namespace Harmony.Web.Services;

using Harmony.ApplicationCore.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public sealed class UpdateBackgroundService : BackgroundService
{
    private readonly TimeSpan _checkInterval;
    private readonly TimeSpan _startupDelay;

    private readonly IUpdateCheckService _updateCheckService;
    private readonly IUpdateNotificationState _notificationState;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<UpdateBackgroundService> _logger;

    public UpdateBackgroundService(
        IUpdateCheckService updateCheckService,
        IUpdateNotificationState notificationState,
        ISettingsService settingsService,
        ILogger<UpdateBackgroundService> logger)
    {
        _updateCheckService = updateCheckService;
        _notificationState = notificationState;
        _settingsService = settingsService;
        _logger = logger;
        _startupDelay = TimeSpan.FromSeconds(10);
        _checkInterval = TimeSpan.FromHours(1);
    }

    internal UpdateBackgroundService(
        IUpdateCheckService updateCheckService,
        IUpdateNotificationState notificationState,
        ISettingsService settingsService,
        ILogger<UpdateBackgroundService> logger,
        TimeSpan startupDelay,
        TimeSpan checkInterval)
        : this(updateCheckService, notificationState, settingsService, logger)
    {
        _startupDelay = startupDelay;
        _checkInterval = checkInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Give the app time to fully start before the first check
        await Task.Delay(_startupDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunCheckIfEnabledAsync(stoppingToken);
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task RunCheckIfEnabledAsync(CancellationToken cancellationToken)
    {
        try
        {
            var enabled = await _settingsService.GetUpdateChecksEnabledAsync(cancellationToken);
            if (!enabled)
            {
                _logger.LogDebug("Software update checks are disabled; skipping.");
                return;
            }

            _logger.LogDebug("Checking for software updates…");

            var includePreReleases = await _settingsService.GetIncludePreReleasesAsync(cancellationToken);
            var latestRelease = await FindLatestReleaseAsync(includePreReleases, cancellationToken);
            _notificationState.SetUpdate(latestRelease, DateTime.Now);

            if (latestRelease is not null)
                _logger.LogInformation("Software update available: {TagName}.", latestRelease.TagName);
            else
                _logger.LogDebug("No software update available.");
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown — do not log as error
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during software update check.");
        }
    }

    private async Task<ReleaseInfo?> FindLatestReleaseAsync(bool includePreReleases, CancellationToken cancellationToken)
    {
        var currentVersion = GetCurrentVersion();
        var releases = await _updateCheckService.GetReleasesAsync(cancellationToken);

        return releases
            .Where(r => !r.IsPreRelease || includePreReleases)
            .Where(r => CompareVersions(r.Version, currentVersion) > 0)
            .OrderByDescending(r => r.Version)
            .FirstOrDefault();
    }

    private static Version GetCurrentVersion()
    {
        var assemblyVersion = typeof(Program).Assembly.GetName().Version;
        return assemblyVersion is null
            ? new Version(0, 0, 0)
            : new Version(assemblyVersion.Major, assemblyVersion.Minor, assemblyVersion.Build);
    }

    private static int CompareVersions(Version candidate, Version current)
    {
        // Compare only Major.Minor.Build (ignore revision)
        var candidateNorm = new Version(candidate.Major, candidate.Minor, Math.Max(candidate.Build, 0));
        var currentNorm = new Version(current.Major, current.Minor, Math.Max(current.Build, 0));
        return candidateNorm.CompareTo(currentNorm);
    }
}
