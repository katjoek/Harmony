using Harmony.ApplicationCore.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Harmony.Infrastructure.Services;

public sealed class DatabaseConnectionStringProvider : IDatabaseConnectionStringProvider
{
    private readonly ISettingsService _settingsService;
    private readonly IConfiguration _configuration;

    public DatabaseConnectionStringProvider(ISettingsService settingsService, IConfiguration configuration)
    {
        _settingsService = settingsService;
        _configuration = configuration;
    }

    public string GetConnectionString()
    {
        // Try to get database directory from settings
        var databaseDirectory = _settingsService.GetDatabaseDirectoryAsync().GetAwaiter().GetResult();
        
        if (string.IsNullOrWhiteSpace(databaseDirectory))
        {
            throw new InvalidOperationException(
                "Database directory is not configured. Please configure the database directory before accessing the database.");
        }
        
        // Use configured directory
        var dbPath = Path.Combine(databaseDirectory, "harmony.db");
        
        // Ensure directory exists
        if (!Directory.Exists(databaseDirectory))
        {
            try
            {
                Directory.CreateDirectory(databaseDirectory);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to create database directory '{databaseDirectory}': {ex.Message}", ex);
            }
        }
        
        return $"Data Source={dbPath}";
    }
}
