using Harmony.ApplicationCore.Interfaces;
using Harmony.Infrastructure.Data;
using Harmony.Infrastructure.Repositories;
using Harmony.Infrastructure.Services;
using Harmony.Web.Commands;
using Harmony.Web.Services;
using LiteBus.Messaging.Extensions.MicrosoftDependencyInjection;
using LiteBus.Commands.Extensions.MicrosoftDependencyInjection;
using LiteBus.Queries.Extensions.MicrosoftDependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

const int maxFileSize = 10 * 1024 * 1024;

// Configure file upload size limits (for database backup uploads)
// Default is 500 KB, increase to 100 MB to accommodate database backups
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxFileSize;
    options.ValueLengthLimit = maxFileSize;
    options.ValueCountLimit = 10; // Maximum number of form values
});

// Configure Kestrel server limits for larger request bodies
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = maxFileSize;
});

// Configure Blazor Server hub options for larger messages
builder.Services.Configure<Microsoft.AspNetCore.SignalR.HubOptions>(options =>
{
    options.MaximumReceiveMessageSize = maxFileSize;
});

// Register settings service as singleton (file-based, shared across requests)
builder.Services.AddSingleton<ISettingsService, SettingsService>();

// Register connection string provider
builder.Services.AddScoped<IDatabaseConnectionStringProvider, Harmony.Infrastructure.Services.DatabaseConnectionStringProvider>();

// Add Entity Framework with dynamic connection string from settings (resolved per request)
// Register DbContext using a factory pattern to defer connection string resolution
// until the DbContext is actually created (prevents database access if directory not configured)
builder.Services.AddScoped<HarmonyDbContext>(serviceProvider =>
{
    var connectionStringProvider = serviceProvider.GetRequiredService<IDatabaseConnectionStringProvider>();
    // During service registration, we can use GetAwaiter().GetResult() safely
    // as we're not in a Blazor Server synchronization context
    var connectionString = connectionStringProvider.GetConnectionStringAsync().GetAwaiter().GetResult();
    
    var optionsBuilder = new DbContextOptionsBuilder<HarmonyDbContext>();
    optionsBuilder.UseSqlite(connectionString);
    
    return new HarmonyDbContext(optionsBuilder.Options);
});

// Add LiteBus
builder.Services.AddLiteBus(config =>
{
    var appAssembly = typeof(Harmony.ApplicationCore.Commands.Persons.CreatePersonCommand).Assembly;
    config.AddCommandModule(module => module.RegisterFromAssembly(appAssembly));
    config.AddQueryModule(module => module.RegisterFromAssembly(appAssembly));
});

// Add repositories and services
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<IReportService, ReportService>();
 builder.Services.AddScoped<IDatabaseBackupService, DatabaseBackupService>();
builder.Services.AddScoped<DataSeeder>();
builder.Services.AddScoped<SeedDataCommand>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// Ensure database is created (only if directory is configured)
using (var scope = app.Services.CreateScope())
{
    var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
    var isConfigured = await settingsService.IsDatabaseDirectoryConfiguredAsync();
    
    if (isConfigured)
    {
        var context = scope.ServiceProvider.GetRequiredService<HarmonyDbContext>();
        context.Database.EnsureCreated();
    }
}

// Check for seed command line argument
if (args.Length > 0 && args[0] == "seed")
{
    using var scope = app.Services.CreateScope();
    var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
    var isConfigured = await settingsService.IsDatabaseDirectoryConfiguredAsync();
    
    if (!isConfigured)
    {
        Console.WriteLine("ERROR: Database directory is not configured. Please configure the database directory first.");
        Console.WriteLine("Run the application and configure the database directory through the web interface.");
        return;
    }
    
    Console.WriteLine("Seeding database...");
    var seedCommand = scope.ServiceProvider.GetRequiredService<SeedDataCommand>();
    await seedCommand.ExecuteAsync();
    Console.WriteLine("Seeding completed. Exiting application.");
    return;
}

#if RELEASE
// Open default browser to Harmony homepage in Release builds only
if (!app.Environment.IsDevelopment())
{
    // Determine the URL to open - use the actual listening URL
    // app.Urls is populated from:
    // 1. Command-line arguments: --urls "http://localhost:8080"
    // 2. Environment variable: ASPNETCORE_URLS="http://localhost:8080"
    // 3. Configuration: appsettings.json "Urls" key
    string url = "http://localhost:5000"; // Default fallback URL
    
    // Try to get URL from app.Urls (includes command-line --urls, environment variables, and config)
    if (app.Urls.Count > 0)
    {
        // Prefer HTTP over HTTPS, or use the first available
        url = app.Urls.FirstOrDefault(u => u.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) 
              ?? app.Urls.First();
    }
    else
    {
        // Fall back to configuration (shouldn't normally be needed as app.Urls should contain these)
        var urls = builder.Configuration["Urls"] ?? builder.Configuration["ASPNETCORE_URLS"];
        if (!string.IsNullOrEmpty(urls))
        {
            // Parse URLs from the semicolon-separated list
            var urlList = urls.Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (urlList.Length > 0)
            {
                // Prefer HTTP over HTTPS
                url = urlList.FirstOrDefault(u => u.Trim().StartsWith("http://", StringComparison.OrdinalIgnoreCase))?.Trim()
                      ?? urlList.First().Trim();
            }
        }
    }
    
    // Ensure URL has protocol
    if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
        !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
    {
        url = "http://" + url;
    }
    
    // Launch browser after a short delay to ensure server is ready
    _ = Task.Run(async () =>
    {
        await Task.Delay(2000); // Wait 2 seconds for server to start
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // Silently fail if browser cannot be opened
            Console.WriteLine($"Could not open browser: {ex.Message}");
        }
    });
}
#endif

app.Run();
