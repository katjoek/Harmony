using System.Text.Json;
using Harmony.ApplicationCore.Interfaces;

namespace Harmony.Infrastructure.Services;

public sealed class SettingsService : ISettingsService
{
    private readonly string _settingsFilePath;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public SettingsService()
    {
        var appDirectory = AppContext.BaseDirectory;
        _settingsFilePath = Path.Combine(appDirectory, "harmony.settings.json");
    }

    public async Task<string?> GetDatabaseDirectoryAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(_settingsFilePath))
                return null;

            var json = await File.ReadAllTextAsync(_settingsFilePath, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            var settings = JsonSerializer.Deserialize<Settings>(json);
            return settings?.DatabaseDirectory;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SetDatabaseDirectoryAsync(string directory, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var settings = new Settings { DatabaseDirectory = directory };
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, json, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> IsDatabaseDirectoryConfiguredAsync(CancellationToken cancellationToken = default)
    {
        var directory = await GetDatabaseDirectoryAsync(cancellationToken);
        return !string.IsNullOrWhiteSpace(directory);
    }

    private sealed class Settings
    {
        public string? DatabaseDirectory { get; set; }
    }
}
