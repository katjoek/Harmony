namespace Harmony.Infrastructure.Services;

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Harmony.ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;

public sealed class GitHubUpdateCheckService : IUpdateCheckService
{
    private const string ReleasesUrl = "repos/katjoek/Harmony/releases";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GitHubUpdateCheckService> _logger;

    public GitHubUpdateCheckService(IHttpClientFactory httpClientFactory, ILogger<GitHubUpdateCheckService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ReleaseInfo>> GetReleasesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("GitHub");
            var githubReleases = await client.GetFromJsonAsync<List<GitHubRelease>>(ReleasesUrl, cancellationToken);

            if (githubReleases is null)
                return [];

            var results = new List<ReleaseInfo>();
            foreach (var release in githubReleases)
            {
                if (!TryParseVersion(release.TagName, out var version))
                    continue;

                var exeAsset = release.Assets?.FirstOrDefault(a =>
                    a.Name?.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) == true);

                results.Add(new ReleaseInfo(
                    Version: version,
                    TagName: release.TagName ?? string.Empty,
                    IsPreRelease: release.Prerelease,
                    InstallerDownloadUrl: exeAsset?.BrowserDownloadUrl,
                    InstallerFileName: exeAsset?.Name));
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve releases from GitHub.");
            return [];
        }
    }

    public async Task<string> DownloadInstallerAsync(string downloadUrl, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(downloadUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var downloadsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads");

        Directory.CreateDirectory(downloadsFolder);
        var targetPath = Path.Combine(downloadsFolder, fileName);

        var client = _httpClientFactory.CreateClient("GitHub");
        using var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fileStream, cancellationToken);

        _logger.LogInformation("Installer downloaded to {Path}.", targetPath);
        return targetPath;
    }

    private static bool TryParseVersion(string? tagName, out Version version)
    {
        version = new Version(0, 0, 0);
        if (string.IsNullOrWhiteSpace(tagName))
            return false;

        var versionString = tagName.TrimStart('v', 'V');
        return Version.TryParse(versionString, out version!);
    }

    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string? TagName { get; set; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; set; }

        [JsonPropertyName("assets")]
        public List<GitHubAsset>? Assets { get; set; }
    }

    private sealed class GitHubAsset
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("browser_download_url")]
        public string? BrowserDownloadUrl { get; set; }
    }
}
