namespace Harmony.Tests.Infrastructure.Services;

using System.Net;
using System.Net.Http;
using System.Text;
using Harmony.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

public sealed class GitHubUpdateCheckServiceTests
{
    private static GitHubUpdateCheckService CreateService(HttpMessageHandler handler)
    {
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.github.com/") };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("GitHub").Returns(client);
        return new GitHubUpdateCheckService(factory, NullLogger<GitHubUpdateCheckService>.Instance);
    }

    [Fact]
    public async Task GetReleasesAsync_WithValidJson_ReturnsParsedReleases()
    {
        // Arrange
        const string json = """
            [
              { "tag_name": "1.2.0", "prerelease": false, "assets": [] },
              { "tag_name": "1.1.0", "prerelease": false, "assets": [] }
            ]
            """;
        var service = CreateService(new StaticResponseHandler(json));

        // Act
        var releases = await service.GetReleasesAsync(CancellationToken.None);

        // Assert
        Assert.Equal(2, releases.Count);
        Assert.Contains(releases, r => r.Version == new Version(1, 2, 0));
        Assert.Contains(releases, r => r.Version == new Version(1, 1, 0));
    }

    [Fact]
    public async Task GetReleasesAsync_WithVPrefixedTag_StripsPrefixAndParsesVersion()
    {
        // Arrange
        const string json = """[{ "tag_name": "v1.3.0", "prerelease": false, "assets": [] }]""";
        var service = CreateService(new StaticResponseHandler(json));

        // Act
        var releases = await service.GetReleasesAsync(CancellationToken.None);

        // Assert
        Assert.Single(releases);
        Assert.Equal(new Version(1, 3, 0), releases[0].Version);
        Assert.Equal("v1.3.0", releases[0].TagName);
    }

    [Fact]
    public async Task GetReleasesAsync_WithInstallerAsset_PopulatesInstallerFields()
    {
        // Arrange
        const string json = """
            [{
              "tag_name": "1.2.0",
              "prerelease": false,
              "assets": [
                {
                  "name": "Harmony-1.2.0-Setup.exe",
                  "browser_download_url": "https://github.com/katjoek/Harmony/releases/download/1.2.0/Harmony-1.2.0-Setup.exe"
                }
              ]
            }]
            """;
        var service = CreateService(new StaticResponseHandler(json));

        // Act
        var releases = await service.GetReleasesAsync(CancellationToken.None);

        // Assert
        Assert.Single(releases);
        Assert.Equal("Harmony-1.2.0-Setup.exe", releases[0].InstallerFileName);
        Assert.Equal(
            "https://github.com/katjoek/Harmony/releases/download/1.2.0/Harmony-1.2.0-Setup.exe",
            releases[0].InstallerDownloadUrl);
    }

    [Fact]
    public async Task GetReleasesAsync_WithPreReleaseTrue_SetsIsPreReleaseFlag()
    {
        // Arrange
        const string json = """[{ "tag_name": "2.0.0", "prerelease": true, "assets": [] }]""";
        var service = CreateService(new StaticResponseHandler(json));

        // Act
        var releases = await service.GetReleasesAsync(CancellationToken.None);

        // Assert
        Assert.Single(releases);
        Assert.True(releases[0].IsPreRelease);
    }

    [Fact]
    public async Task GetReleasesAsync_WithUnparseableTag_SkipsRelease()
    {
        // Arrange
        const string json = """
            [
              { "tag_name": "nightly-build", "prerelease": false, "assets": [] },
              { "tag_name": "1.2.0", "prerelease": false, "assets": [] }
            ]
            """;
        var service = CreateService(new StaticResponseHandler(json));

        // Act
        var releases = await service.GetReleasesAsync(CancellationToken.None);

        // Assert
        Assert.Single(releases);
        Assert.Equal(new Version(1, 2, 0), releases[0].Version);
    }

    [Fact]
    public async Task GetReleasesAsync_WhenHttpThrows_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateService(new ThrowingHandler());

        // Act
        var releases = await service.GetReleasesAsync(CancellationToken.None);

        // Assert
        Assert.Empty(releases);
    }

    [Fact]
    public async Task GetReleasesAsync_WithNoInstallerAsset_LeavesFieldsNull()
    {
        // Arrange
        const string json = """
            [{
              "tag_name": "1.2.0",
              "prerelease": false,
              "assets": [{ "name": "Harmony-1.2.0-readme.txt", "browser_download_url": "https://example.com/readme.txt" }]
            }]
            """;
        var service = CreateService(new StaticResponseHandler(json));

        // Act
        var releases = await service.GetReleasesAsync(CancellationToken.None);

        // Assert
        Assert.Single(releases);
        Assert.Null(releases[0].InstallerDownloadUrl);
        Assert.Null(releases[0].InstallerFileName);
    }

    private sealed class StaticResponseHandler(string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            throw new HttpRequestException("Network failure");
    }
}
