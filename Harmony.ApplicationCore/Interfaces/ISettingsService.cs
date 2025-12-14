namespace Harmony.ApplicationCore.Interfaces;

public interface ISettingsService
{
    Task<string?> GetDatabaseDirectoryAsync(CancellationToken cancellationToken = default);
    Task SetDatabaseDirectoryAsync(string directory, CancellationToken cancellationToken = default);
    Task<bool> IsDatabaseDirectoryConfiguredAsync(CancellationToken cancellationToken = default);
}
