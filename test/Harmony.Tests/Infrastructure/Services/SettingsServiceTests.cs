namespace Harmony.Tests.Infrastructure.Services;

using Harmony.Infrastructure.Services;
using Xunit;

public sealed class SettingsServiceTests : IDisposable
{
    private readonly string _tempFile;
    private readonly SettingsService _service;

    public SettingsServiceTests()
    {
        _tempFile = Path.Combine(Path.GetTempPath(), $"harmony-test-{Guid.NewGuid()}.settings.json");
        _service = new SettingsService(_tempFile);
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
    }

    [Fact]
    public async Task GetIncludePreReleasesAsync_WhenFileAbsent_ReturnsFalse()
    {
        // Arrange — no file created

        // Act
        var result = await _service.GetIncludePreReleasesAsync(CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetUpdateChecksEnabledAsync_WhenFileAbsent_ReturnsTrue()
    {
        // Arrange — no file created

        // Act
        var result = await _service.GetUpdateChecksEnabledAsync(CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SetAndGetIncludePreReleasesAsync_RoundTrip_PersistsValue()
    {
        // Arrange & Act
        await _service.SetIncludePreReleasesAsync(true, CancellationToken.None);
        var result = await _service.GetIncludePreReleasesAsync(CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SetAndGetUpdateChecksEnabledAsync_RoundTrip_PersistsValue()
    {
        // Arrange & Act
        await _service.SetUpdateChecksEnabledAsync(false, CancellationToken.None);
        var result = await _service.GetUpdateChecksEnabledAsync(CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SetUpdateChecksEnabledAsync_PreservesOtherSettings()
    {
        // Arrange — write IncludePreReleases first
        await _service.SetIncludePreReleasesAsync(true, CancellationToken.None);

        // Act — update only UpdateChecksEnabled
        await _service.SetUpdateChecksEnabledAsync(false, CancellationToken.None);

        // Assert — IncludePreReleases is still true
        var includePreReleases = await _service.GetIncludePreReleasesAsync(CancellationToken.None);
        var updateChecksEnabled = await _service.GetUpdateChecksEnabledAsync(CancellationToken.None);

        Assert.True(includePreReleases);
        Assert.False(updateChecksEnabled);
    }

    [Fact]
    public async Task SetIncludePreReleasesAsync_PreservesOtherSettings()
    {
        // Arrange — disable update checks first
        await _service.SetUpdateChecksEnabledAsync(false, CancellationToken.None);

        // Act — update only IncludePreReleases
        await _service.SetIncludePreReleasesAsync(true, CancellationToken.None);

        // Assert — UpdateChecksEnabled is still false
        var updateChecksEnabled = await _service.GetUpdateChecksEnabledAsync(CancellationToken.None);
        var includePreReleases = await _service.GetIncludePreReleasesAsync(CancellationToken.None);

        Assert.False(updateChecksEnabled);
        Assert.True(includePreReleases);
    }
}
