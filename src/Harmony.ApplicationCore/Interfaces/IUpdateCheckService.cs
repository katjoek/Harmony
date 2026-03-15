namespace Harmony.ApplicationCore.Interfaces;

public sealed record ReleaseInfo(
    Version Version,
    string TagName,
    bool IsPreRelease,
    string? MsiDownloadUrl,
    string? MsiFileName);

public interface IUpdateCheckService
{
    Task<IReadOnlyList<ReleaseInfo>> GetReleasesAsync(CancellationToken cancellationToken = default);
    Task<string> DownloadInstallerAsync(string downloadUrl, string fileName, CancellationToken cancellationToken = default);
}
