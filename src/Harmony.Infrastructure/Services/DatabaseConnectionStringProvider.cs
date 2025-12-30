using Harmony.ApplicationCore.Interfaces;

namespace Harmony.Infrastructure.Services;

public sealed class DatabaseConnectionStringProvider : IDatabaseConnectionStringProvider
{
    private readonly ISettingsService _settingsService;

    public DatabaseConnectionStringProvider(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public async Task<string> GetConnectionStringAsync(CancellationToken cancellationToken = default)
    {
        // Try to get database directory from settings
        var databaseDirectory = await _settingsService.GetDatabaseDirectoryAsync(cancellationToken);
        
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
