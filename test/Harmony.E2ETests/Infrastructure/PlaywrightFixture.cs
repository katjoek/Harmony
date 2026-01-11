namespace Harmony.E2ETests.Infrastructure;

using Microsoft.Playwright;
using Xunit;

/// <summary>
/// xUnit fixture that manages Playwright browser instance lifecycle.
/// Shared across all tests in the collection for better performance.
/// </summary>
public sealed class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        
        // Use Chromium for E2E tests - headless by default
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true // Set to false for debugging
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        Playwright.Dispose();
    }

    /// <summary>
    /// Creates a new browser context with isolated cookies and storage.
    /// Each test should use its own context for isolation.
    /// </summary>
    public async Task<IBrowserContext> CreateContextAsync()
    {
        return await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true // Ignore SSL errors for local testing
        });
    }
}

/// <summary>
/// Collection definition for tests that share the Playwright fixture.
/// </summary>
[CollectionDefinition(nameof(PlaywrightCollection))]
public sealed class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
{
}
