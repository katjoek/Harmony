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
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        
        string settingsDirectory;
        if (appDirectory.StartsWith(userProfile, StringComparison.OrdinalIgnoreCase))
        {
            // Installed in user profile → use AppData (per-user settings)
            settingsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Harmony");
        }
        else
        {
            // Installed system-wide (e.g., Program Files) → use ProgramData (shared settings)
            settingsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Harmony");
        }
        
        Directory.CreateDirectory(settingsDirectory);
        _settingsFilePath = Path.Combine(settingsDirectory, "harmony.settings.json");
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

    public async Task<bool> GetListenOnAllInterfacesAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (!File.Exists(_settingsFilePath))
                return false; // Default: localhost only

            var json = await File.ReadAllTextAsync(_settingsFilePath, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
                return false; // Default: localhost only

            var settings = JsonSerializer.Deserialize<Settings>(json);
            return settings?.ListenOnAllInterfaces ?? false; // Default: localhost only
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SetListenOnAllInterfacesAsync(bool listenOnAllInterfaces, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            Settings settings;
            
            // Load existing settings if file exists
            if (File.Exists(_settingsFilePath))
            {
                var json = await File.ReadAllTextAsync(_settingsFilePath, cancellationToken);
                settings = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
            else
            {
                settings = new Settings();
            }
            
            // Update the setting
            settings.ListenOnAllInterfaces = listenOnAllInterfaces;
            
            // Save settings
            var updatedJson = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, updatedJson, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private sealed class Settings
    {
        public string? DatabaseDirectory { get; set; }
        public bool ListenOnAllInterfaces { get; set; }
    }
}
