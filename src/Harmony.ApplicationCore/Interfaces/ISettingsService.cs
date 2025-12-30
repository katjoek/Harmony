namespace Harmony.ApplicationCore.Interfaces;

public interface ISettingsService
{
    Task<string?> GetDatabaseDirectoryAsync(CancellationToken cancellationToken = default);
    Task SetDatabaseDirectoryAsync(string directory, CancellationToken cancellationToken = default);
    Task<bool> IsDatabaseDirectoryConfiguredAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets whether the server should listen on all network interfaces (0.0.0.0) or localhost only.
    /// Returns true for all interfaces, false for localhost only. Defaults to false (localhost only).
    /// </summary>
    Task<bool> GetListenOnAllInterfacesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sets whether the server should listen on all network interfaces (0.0.0.0) or localhost only.
    /// </summary>
    Task SetListenOnAllInterfacesAsync(bool listenOnAllInterfaces, CancellationToken cancellationToken = default);
}
