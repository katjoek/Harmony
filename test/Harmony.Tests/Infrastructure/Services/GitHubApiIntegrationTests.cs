namespace Harmony.Tests.Infrastructure.Services;

using Harmony.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

/// <summary>
/// Integration tests that call the real GitHub API.
/// Run with: dotnet test --filter "Category=Integration"
/// Excluded from the default test run.
/// </summary>
public sealed class GitHubApiIntegrationTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetReleasesAsync_FromRealGitHub_ReturnsReleasesWithExpectedFields()
    {
        // Arrange
        using var httpClient = new HttpClient { BaseAddress = new Uri("https://api.github.com/") };
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Harmony-UpdateCheck/1.0");

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("GitHub").Returns(httpClient);

        var service = new GitHubUpdateCheckService(factory, NullLogger<GitHubUpdateCheckService>.Instance);

        // Act
        var releases = await service.GetReleasesAsync(CancellationToken.None);

        // Assert — the GitHub repo must have at least one release
        Assert.NotEmpty(releases);

        // Every release must have a tag name
        Assert.All(releases, r => Assert.False(string.IsNullOrWhiteSpace(r.TagName)));

        // At least one release must have a parseable semantic version (v-prefixed tag)
        Assert.Contains(releases, r => r.Version > new Version(0, 0, 0));

        // IsPreRelease field is always populated (bool cannot be null)
        Assert.All(releases, r => Assert.IsType<bool>(r.IsPreRelease));
    }
}
