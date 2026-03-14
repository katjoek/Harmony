namespace Harmony.E2ETests.Infrastructure;

using Xunit;

/// <summary>
/// xUnit fixture that manages the application server lifecycle.
/// Shared across all tests in the collection so the server starts only once.
/// </summary>
public sealed class HarmonyAppFixture : IAsyncLifetime
{
    public HarmonyWebApplicationFactory Factory { get; private set; } = null!;
    public string BaseUrl => Factory.BaseUrl;

    public async Task InitializeAsync()
    {
        Factory = new HarmonyWebApplicationFactory();
        _ = Factory.CreateClient(); // triggers host startup
        await Factory.InitializeDatabaseAsync();
    }

    public async Task DisposeAsync() => await Factory.DisposeAsync();
}
