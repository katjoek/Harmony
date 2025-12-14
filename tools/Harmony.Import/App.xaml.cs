using System.IO;
using System.Text;
using System.Windows;
using Harmony.ApplicationCore.Interfaces;
using Harmony.Import.Services;
using Harmony.Import.ViewModels;
using Harmony.Infrastructure.Data;
using Harmony.Infrastructure.Repositories;
using Harmony.Infrastructure.Services;
using LiteBus.Messaging.Extensions.MicrosoftDependencyInjection;
using LiteBus.Commands.Extensions.MicrosoftDependencyInjection;
using LiteBus.Queries.Extensions.MicrosoftDependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Harmony.Import;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Register code page encodings (required for Windows-1252 and other code page encodings in .NET Core/.NET 5+)
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        // Build service collection
        var services = new ServiceCollection();

        // Configure database
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=%APPDATA%\\Harmony\\harmony.db";
        var expandedConnectionString = Environment.ExpandEnvironmentVariables(connectionString);

        // Extract database path for backup service
        var dataSourcePrefix = "Data Source=";
        var idx = expandedConnectionString.IndexOf(dataSourcePrefix, StringComparison.OrdinalIgnoreCase);
        string databasePath;
        if (idx >= 0)
        {
            databasePath = expandedConnectionString.Substring(idx + dataSourcePrefix.Length).Trim();
            var directory = Path.GetDirectoryName(databasePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        else
        {
            databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Harmony", "harmony.db");
        }

        services.AddDbContext<HarmonyDbContext>(options => options.UseSqlite(expandedConnectionString));

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add LiteBus
        services.AddLiteBus(builder =>
        {
            var appAssembly = typeof(Harmony.ApplicationCore.Commands.Persons.CreatePersonCommand).Assembly;
            builder.AddCommandModule(module => module.RegisterFromAssembly(appAssembly));
            builder.AddQueryModule(module => module.RegisterFromAssembly(appAssembly));
        });

        // Add repositories and services
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IGroupRepository, Harmony.Infrastructure.Repositories.GroupRepository>();
        services.AddScoped<IMembershipService, MembershipService>();

        // Add import services
        services.AddSingleton<ICsvParserService, CsvParserService>();
        services.AddSingleton<Harmony.Import.Services.IDatabaseBackupService>(sp => new Harmony.Import.Services.DatabaseBackupService(databasePath));
        services.AddScoped<IImportService, ImportService>();

        // Add ViewModels
        services.AddTransient<MainWindowViewModel>();

        _serviceProvider = services.BuildServiceProvider();

        // Database will be initialized lazily when import starts (after backup)
        // This prevents file locking issues during backup

        // Create and show main window
        var mainWindow = new MainWindow();
        mainWindow.DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        base.OnExit(e);
    }
}
