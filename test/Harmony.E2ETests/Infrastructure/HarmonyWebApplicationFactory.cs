namespace Harmony.E2ETests.Infrastructure;

using Harmony.ApplicationCore.Interfaces;
using Harmony.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Custom WebApplicationFactory that configures the application for E2E testing
/// with an isolated SQLite database file and a real HTTP server for Playwright.
/// </summary>
public sealed class HarmonyWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _testDatabasePath;
    private readonly string _testDatabaseDirectory;
    private IHost? _host;
    private readonly int _port;

    public string BaseUrl => $"http://localhost:{_port}";

    public HarmonyWebApplicationFactory()
    {
        // Create a unique temp directory for each test run
        _testDatabaseDirectory = Path.Combine(Path.GetTempPath(), $"HarmonyTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDatabaseDirectory);
        _testDatabasePath = Path.Combine(_testDatabaseDirectory, "harmony_test.db");
        
        // Use a random available port
        _port = GetRandomAvailablePort();
    }

    private static int GetRandomAvailablePort()
    {
        // Use 0 to get a random available port from the OS
        using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseUrls(BaseUrl);

        builder.ConfigureServices(services =>
        {
            // Remove the existing SettingsService registration
            var settingsDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ISettingsService));
            if (settingsDescriptor is not null)
            {
                services.Remove(settingsDescriptor);
            }

            // Add test settings service that uses our temp directory
            services.AddSingleton<ISettingsService>(new TestSettingsService(_testDatabaseDirectory));

            // Remove the existing DbContext registration
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(HarmonyDbContext));
            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Add DbContext with SQLite file database
            services.AddScoped<HarmonyDbContext>(sp =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<HarmonyDbContext>();
                optionsBuilder.UseSqlite($"Data Source={_testDatabasePath}");
                return new HarmonyDbContext(optionsBuilder.Options);
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Create the host that will be used for testing
        var testHost = base.CreateHost(builder);

        // Create and start a real host that listens on the network
        builder.ConfigureWebHost(webHostBuilder =>
        {
            webHostBuilder.UseKestrel();
            webHostBuilder.UseUrls(BaseUrl);
        });

        _host = builder.Build();
        _host.Start();

        return testHost;
    }

    /// <summary>
    /// Ensures the test database is created and migrations are applied.
    /// Call this before running tests.
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<HarmonyDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _host?.StopAsync().GetAwaiter().GetResult();
            _host?.Dispose();
        }

        base.Dispose(disposing);

        if (disposing)
        {
            // Clean up test database directory
            try
            {
                if (Directory.Exists(_testDatabaseDirectory))
                {
                    Directory.Delete(_testDatabaseDirectory, recursive: true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
