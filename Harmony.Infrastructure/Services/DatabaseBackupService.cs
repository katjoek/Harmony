using Harmony.ApplicationCore.Interfaces;

namespace Harmony.Infrastructure.Services;

public sealed class DatabaseBackupService : IDatabaseBackupService
{
    private readonly IDatabaseConnectionStringProvider _connectionStringProvider;

    public DatabaseBackupService(IDatabaseConnectionStringProvider connectionStringProvider)
    {
        _connectionStringProvider = connectionStringProvider;
    }

    public async Task<string> GetDatabasePathAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = await _connectionStringProvider.GetConnectionStringAsync(cancellationToken)
            .ConfigureAwait(false);
        var dataSourcePrefix = "Data Source=";
        var idx = connectionString.IndexOf(dataSourcePrefix, StringComparison.OrdinalIgnoreCase);
        
        if (idx >= 0)
        {
            return connectionString.Substring(idx + dataSourcePrefix.Length).Trim();
        }
        
        throw new InvalidOperationException("Could not extract database path from connection string.");
    }

    public async Task<string> CreateBackupAsync(CancellationToken cancellationToken = default)
    {
        var databasePath = await GetDatabasePathAsync(cancellationToken);
        
        if (!File.Exists(databasePath))
        {
            throw new InvalidOperationException("Database file does not exist.");
        }

        var directory = Path.GetDirectoryName(databasePath) ?? string.Empty;
        var fileName = Path.GetFileNameWithoutExtension(databasePath);
        var extension = Path.GetExtension(databasePath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"{fileName}_backup_{timestamp}{extension}";
        var backupPath = Path.Combine(directory, backupFileName);

        // Copy database file to backup location
        await Task.Run(() => File.Copy(databasePath, backupPath, overwrite: true), cancellationToken);

        return backupPath;
    }

    public async Task RestoreBackupAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(backupFilePath))
        {
            throw new FileNotFoundException("Backup file not found.", backupFilePath);
        }

        var databasePath = await GetDatabasePathAsync(cancellationToken);
        var directory = Path.GetDirectoryName(databasePath) ?? string.Empty;

        // Ensure directory exists
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // For SQLite, we need to ensure all connections are closed before replacing the file
        // Use a robust file replacement strategy: delete old file, then copy new one
        // Retry a few times in case of file locks
        await Task.Run(() =>
        {
            const int maxRetries = 5;
            const int delayMs = 100;
            
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    // If database file exists, delete it first to release any locks
                    if (File.Exists(databasePath))
                    {
                        File.Delete(databasePath);
                    }
                    
                    // Small delay to ensure file system has released the file
                    if (attempt > 0)
                    {
                        Thread.Sleep(delayMs * attempt);
                    }
                    
                    // Copy backup file to database location
                    File.Copy(backupFilePath, databasePath, overwrite: false);
                    
                    // Success - break out of retry loop
                    return;
                }
                catch (IOException) when (attempt < maxRetries - 1)
                {
                    // File might still be locked, retry after delay
                    Thread.Sleep(delayMs * (attempt + 1));
                }
            }
            
            // If we get here, all retries failed
            throw new InvalidOperationException(
                $"Failed to restore backup after {maxRetries} attempts. The database file may be locked by another process.");
        }, cancellationToken);
    }

    public async Task<byte[]> GetBackupFileAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(backupFilePath))
        {
            throw new FileNotFoundException("Backup file not found.", backupFilePath);
        }

        return await File.ReadAllBytesAsync(backupFilePath, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> ListBackupsAsync(CancellationToken cancellationToken = default)
    {
        var databasePath = await GetDatabasePathAsync(cancellationToken);
        var directory = Path.GetDirectoryName(databasePath) ?? string.Empty;
        
        if (!Directory.Exists(directory))
        {
            return Array.Empty<string>();
        }

        var fileName = Path.GetFileNameWithoutExtension(databasePath);
        var extension = Path.GetExtension(databasePath);
        var pattern = $"{fileName}_backup_*{extension}";

        return await Task.Run(() =>
        {
            var backups = Directory.GetFiles(directory, pattern)
                .OrderByDescending(f => File.GetCreationTime(f))
                .Select(f => Path.GetFileName(f) ?? string.Empty)
                .Where(f => !string.IsNullOrEmpty(f))
                .ToList();
            
            return backups.AsReadOnly();
        }, cancellationToken);
    }

    public async Task DeleteBackupAsync(string backupFileName, CancellationToken cancellationToken = default)
    {
        var databasePath = await GetDatabasePathAsync(cancellationToken);
        var directory = Path.GetDirectoryName(databasePath) ?? string.Empty;
        var backupPath = Path.Combine(directory, backupFileName);

        if (!File.Exists(backupPath))
        {
            throw new FileNotFoundException("Backup file not found.", backupPath);
        }

        await Task.Run(() => File.Delete(backupPath), cancellationToken);
    }
}
