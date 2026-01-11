namespace Harmony.E2ETests.Infrastructure;

using Harmony.ApplicationCore.Interfaces;

/// <summary>
/// A test implementation of ISettingsService that uses a temporary directory
/// for the database, ensuring test isolation.
/// </summary>
public sealed class TestSettingsService : ISettingsService
{
    private readonly string _testDatabaseDirectory;

    public TestSettingsService(string testDatabaseDirectory)
    {
        _testDatabaseDirectory = testDatabaseDirectory;
    }

    public Task<string?> GetDatabaseDirectoryAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<string?>(_testDatabaseDirectory);

    public Task SetDatabaseDirectoryAsync(string directory, CancellationToken cancellationToken = default)
        => Task.CompletedTask; // No-op for tests

    public Task<bool> IsDatabaseDirectoryConfiguredAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(true); // Always configured for tests

    public Task<bool> GetListenOnAllInterfacesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(false); // Localhost only for tests

    public Task SetListenOnAllInterfacesAsync(bool listenOnAllInterfaces, CancellationToken cancellationToken = default)
        => Task.CompletedTask; // No-op for tests
}
