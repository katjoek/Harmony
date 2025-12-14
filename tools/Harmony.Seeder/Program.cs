using System;
using System.IO;
using Harmony.ApplicationCore.Interfaces;
using Harmony.Infrastructure.Data;
using Harmony.Infrastructure.Repositories;
using Harmony.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        var env = context.HostingEnvironment;

        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

        // Also load the Web app's appsettings to share the same connection string
        var baseDir = AppContext.BaseDirectory;
        var webAppSettingsPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "Harmony.Web", "appsettings.json"));
        if (File.Exists(webAppSettingsPath))
        {
            config.AddJsonFile(webAppSettingsPath, optional: true);
        }
    })
    .ConfigureServices((context, services) =>
    {
        var raw = context.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(raw))
        {
            // Fallback: point directly at the Web project's SQLite file
            var baseDir = AppContext.BaseDirectory;
            var webDbPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "Harmony.Web", "harmony.db"));
            raw = $"Data Source={webDbPath}";
        }

        var expanded = Environment.ExpandEnvironmentVariables(raw);

        // Ensure directory exists for the SQLite file
        const string dataSourcePrefix = "Data Source=";
        var idx = expanded.IndexOf(dataSourcePrefix, StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            var pathPart = expanded.Substring(idx + dataSourcePrefix.Length).Trim();
            var directory = Path.GetDirectoryName(pathPart);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        services.AddDbContext<HarmonyDbContext>(options => options.UseSqlite(expanded));

        // Repositories and services
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IMembershipService, MembershipService>();

        // Seeder
        services.AddScoped<DataSeeder>();

        services.AddLogging();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HarmonyDbContext>();
    db.Database.EnsureCreated();

    // Seed persons and groups (ensures coordinators during seeding)
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}


