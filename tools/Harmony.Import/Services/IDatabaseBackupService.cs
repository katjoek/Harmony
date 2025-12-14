namespace Harmony.Import.Services;

public interface IDatabaseBackupService
{
    Task<bool> BackupDatabaseAsync();
}
