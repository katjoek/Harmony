namespace Harmony.ApplicationCore.Interfaces;

public interface IDatabaseBackupService
{
    Task<string> CreateBackupAsync(CancellationToken cancellationToken = default);
    Task RestoreBackupAsync(string backupFilePath, CancellationToken cancellationToken = default);
    Task<byte[]> GetBackupFileAsync(string backupFilePath, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListBackupsAsync(CancellationToken cancellationToken = default);
    Task<string> GetDatabasePathAsync(CancellationToken cancellationToken = default);
    Task DeleteBackupAsync(string backupFileName, CancellationToken cancellationToken = default);
}
