namespace Harmony.ApplicationCore.Interfaces;

public interface IDatabaseConnectionStringProvider
{
    Task<string> GetConnectionStringAsync(CancellationToken cancellationToken = default);
}
