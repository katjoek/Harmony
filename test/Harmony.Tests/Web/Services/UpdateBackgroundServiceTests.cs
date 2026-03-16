namespace Harmony.Tests.Web.Services;

using Harmony.ApplicationCore.Interfaces;
using Harmony.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

public sealed class UpdateBackgroundServiceTests
{
    // Current app version is 1.2.1 — any release > 1.2.1 counts as an update.
    private static readonly Version NewerVersion = new(1, 3, 0);
    private static readonly Version SameVersion = new(1, 2, 1);
    private static readonly Version OlderVersion = new(1, 1, 0);

    private static readonly ReleaseInfo NewerRelease =
        new(NewerVersion, "v1.3.0", IsPreRelease: false, InstallerDownloadUrl: null, InstallerFileName: null);

    private static readonly ReleaseInfo SameRelease =
        new(SameVersion, "v1.2.1", IsPreRelease: false, InstallerDownloadUrl: null, InstallerFileName: null);

    private static readonly ReleaseInfo OlderRelease =
        new(OlderVersion, "v1.1.0", IsPreRelease: false, InstallerDownloadUrl: null, InstallerFileName: null);

    private static readonly ReleaseInfo PreReleaseNewer =
        new(NewerVersion, "v1.3.0-beta1", IsPreRelease: true, InstallerDownloadUrl: null, InstallerFileName: null);

    private static UpdateBackgroundService CreateService(
        IUpdateCheckService updateCheckService,
        IUpdateNotificationState notificationState,
        ISettingsService settingsService)
    {
        return new UpdateBackgroundService(
            updateCheckService,
            notificationState,
            settingsService,
            NullLogger<UpdateBackgroundService>.Instance,
            startupDelay: TimeSpan.Zero,
            checkInterval: TimeSpan.FromDays(1));
    }

    [Fact]
    public async Task ExecuteAsync_WhenNewerVersionExists_SetsUpdateOnState()
    {
        // Arrange
        var updateCheckService = Substitute.For<IUpdateCheckService>();
        var notificationState = Substitute.For<IUpdateNotificationState>();
        var settingsService = Substitute.For<ISettingsService>();

        updateCheckService.GetReleasesAsync(Arg.Any<CancellationToken>())
            .Returns([NewerRelease, OlderRelease]);
        settingsService.GetUpdateChecksEnabledAsync(Arg.Any<CancellationToken>()).Returns(true);
        settingsService.GetIncludePreReleasesAsync(Arg.Any<CancellationToken>()).Returns(false);

        var service = CreateService(updateCheckService, notificationState, settingsService);

        // Act
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await RunOneIterationAsync(service, cts.Token);

        // Assert
        notificationState.Received(1).SetUpdate(
            Arg.Is<ReleaseInfo?>(r => r != null && r.Version == NewerVersion),
            Arg.Any<DateTime>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoNewerVersionExists_SetsNullOnState()
    {
        // Arrange
        var updateCheckService = Substitute.For<IUpdateCheckService>();
        var notificationState = Substitute.For<IUpdateNotificationState>();
        var settingsService = Substitute.For<ISettingsService>();

        updateCheckService.GetReleasesAsync(Arg.Any<CancellationToken>())
            .Returns([SameRelease, OlderRelease]);
        settingsService.GetUpdateChecksEnabledAsync(Arg.Any<CancellationToken>()).Returns(true);
        settingsService.GetIncludePreReleasesAsync(Arg.Any<CancellationToken>()).Returns(false);

        var service = CreateService(updateCheckService, notificationState, settingsService);

        // Act
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await RunOneIterationAsync(service, cts.Token);

        // Assert
        notificationState.Received(1).SetUpdate(
            Arg.Is<ReleaseInfo?>(r => r == null),
            Arg.Any<DateTime>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenChecksDisabled_DoesNotCallGetReleases()
    {
        // Arrange
        var updateCheckService = Substitute.For<IUpdateCheckService>();
        var notificationState = Substitute.For<IUpdateNotificationState>();
        var settingsService = Substitute.For<ISettingsService>();

        settingsService.GetUpdateChecksEnabledAsync(Arg.Any<CancellationToken>()).Returns(false);

        var service = CreateService(updateCheckService, notificationState, settingsService);

        // Act
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await RunOneIterationAsync(service, cts.Token);

        // Assert
        await updateCheckService.DidNotReceive().GetReleasesAsync(Arg.Any<CancellationToken>());
        notificationState.DidNotReceive().SetUpdate(Arg.Any<ReleaseInfo?>(), Arg.Any<DateTime>());
    }

    [Fact]
    public async Task ExecuteAsync_WithPreReleaseAndIncludeTrue_SelectsPreRelease()
    {
        // Arrange
        var updateCheckService = Substitute.For<IUpdateCheckService>();
        var notificationState = Substitute.For<IUpdateNotificationState>();
        var settingsService = Substitute.For<ISettingsService>();

        updateCheckService.GetReleasesAsync(Arg.Any<CancellationToken>())
            .Returns([PreReleaseNewer]);
        settingsService.GetUpdateChecksEnabledAsync(Arg.Any<CancellationToken>()).Returns(true);
        settingsService.GetIncludePreReleasesAsync(Arg.Any<CancellationToken>()).Returns(true);

        var service = CreateService(updateCheckService, notificationState, settingsService);

        // Act
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await RunOneIterationAsync(service, cts.Token);

        // Assert
        notificationState.Received(1).SetUpdate(
            Arg.Is<ReleaseInfo?>(r => r != null && r.IsPreRelease),
            Arg.Any<DateTime>());
    }

    [Fact]
    public async Task ExecuteAsync_WithPreReleaseAndIncludeFalse_IgnoresPreRelease()
    {
        // Arrange
        var updateCheckService = Substitute.For<IUpdateCheckService>();
        var notificationState = Substitute.For<IUpdateNotificationState>();
        var settingsService = Substitute.For<ISettingsService>();

        updateCheckService.GetReleasesAsync(Arg.Any<CancellationToken>())
            .Returns([PreReleaseNewer]);
        settingsService.GetUpdateChecksEnabledAsync(Arg.Any<CancellationToken>()).Returns(true);
        settingsService.GetIncludePreReleasesAsync(Arg.Any<CancellationToken>()).Returns(false);

        var service = CreateService(updateCheckService, notificationState, settingsService);

        // Act
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await RunOneIterationAsync(service, cts.Token);

        // Assert
        notificationState.Received(1).SetUpdate(
            Arg.Is<ReleaseInfo?>(r => r == null),
            Arg.Any<DateTime>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenMultipleNewerVersions_SelectsHighestVersion()
    {
        // Arrange
        var highest = new ReleaseInfo(new Version(2, 0, 0), "v2.0.0", false, null, null);
        var middle = new ReleaseInfo(new Version(1, 4, 0), "v1.4.0", false, null, null);

        var updateCheckService = Substitute.For<IUpdateCheckService>();
        var notificationState = Substitute.For<IUpdateNotificationState>();
        var settingsService = Substitute.For<ISettingsService>();

        updateCheckService.GetReleasesAsync(Arg.Any<CancellationToken>())
            .Returns([middle, highest, NewerRelease]);
        settingsService.GetUpdateChecksEnabledAsync(Arg.Any<CancellationToken>()).Returns(true);
        settingsService.GetIncludePreReleasesAsync(Arg.Any<CancellationToken>()).Returns(false);

        var service = CreateService(updateCheckService, notificationState, settingsService);

        // Act
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await RunOneIterationAsync(service, cts.Token);

        // Assert
        notificationState.Received(1).SetUpdate(
            Arg.Is<ReleaseInfo?>(r => r != null && r.Version == new Version(2, 0, 0)),
            Arg.Any<DateTime>());
    }

    /// <summary>
    /// Starts the background service, waits for the first iteration to complete
    /// (startup delay is zero), then cancels.
    /// </summary>
    private static async Task RunOneIterationAsync(
        UpdateBackgroundService service,
        CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Start the service; because startupDelay=0 and checkInterval=1 day, exactly one
        // check runs before we cancel after a short wait.
        var task = service.StartAsync(cts.Token);
        await task;

        // Give the single iteration time to complete
        await Task.Delay(200, CancellationToken.None);

        await cts.CancelAsync();

        try
        {
            await service.StopAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // expected on stop
        }
    }
}
