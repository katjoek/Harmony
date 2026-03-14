namespace Harmony.Web;

using Harmony.ApplicationCore.Commands.Persons;
using Harmony.ApplicationCore.Interfaces;
using Harmony.Infrastructure.Data;
using Harmony.Infrastructure.Repositories;
using Harmony.Infrastructure.Services;
using Harmony.Web.Commands;
using Harmony.Web.Services;
using LiteBus.Commands.Extensions.MicrosoftDependencyInjection;
using LiteBus.Messaging.Extensions.MicrosoftDependencyInjection;
using LiteBus.Queries.Extensions.MicrosoftDependencyInjection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

internal static class ServiceCollectionExtensions
{
    private const int MaxFileSize = 10 * 1024 * 1024;

    internal static IServiceCollection AddBlazorInfrastructure(this IServiceCollection services)
    {
        services.AddRazorPages();
        services.AddServerSideBlazor();

        services.Configure<FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = MaxFileSize;
            options.ValueLengthLimit = MaxFileSize;
            options.ValueCountLimit = 10;
        });

        services.Configure<KestrelServerOptions>(options =>
            options.Limits.MaxRequestBodySize = MaxFileSize);

        services.Configure<HubOptions>(options =>
            options.MaximumReceiveMessageSize = MaxFileSize);

        return services;
    }

    internal static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddScoped<IDatabaseConnectionStringProvider, DatabaseConnectionStringProvider>();

        services.AddScoped<HarmonyDbContext>(sp =>
        {
            var connectionString = sp.GetRequiredService<IDatabaseConnectionStringProvider>()
                .GetConnectionStringAsync().GetAwaiter().GetResult();
            var options = new DbContextOptionsBuilder<HarmonyDbContext>()
                .UseSqlite(connectionString)
                .Options;
            return new HarmonyDbContext(options);
        });

        services.AddLiteBus(config =>
        {
            var appAssembly = typeof(CreatePersonCommand).Assembly;
            config.AddCommandModule(module => module.RegisterFromAssembly(appAssembly));
            config.AddQueryModule(module => module.RegisterFromAssembly(appAssembly));
        });

        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IConfigRepository, ConfigRepository>();
        services.AddScoped<IMembershipService, MembershipService>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IReportFileNameBuilder, ReportFileNameBuilder>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IDatabaseBackupService, DatabaseBackupService>();
        services.AddScoped<IDatabaseCleanupService, DatabaseCleanupService>();
        services.AddScoped<DataSeeder>();
        services.AddScoped<SeedDataCommand>();

        return services;
    }
}
