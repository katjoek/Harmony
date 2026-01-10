namespace Harmony.ApplicationCore.Interfaces;

public interface IConfigRepository
{
    Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default);
    Task SetValueAsync(string key, string? value, CancellationToken cancellationToken = default);
}
