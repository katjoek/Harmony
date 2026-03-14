namespace Harmony.E2ETests;

using Harmony.E2ETests.Infrastructure;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

[Collection(nameof(PlaywrightCollection))]
public sealed class PersonsMembershipTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _playwrightFixture;
    private readonly HarmonyAppFixture _appFixture;
    private readonly ITestOutputHelper _output;
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    public PersonsMembershipTests(PlaywrightFixture playwrightFixture, HarmonyAppFixture appFixture, ITestOutputHelper output)
    {
        _playwrightFixture = playwrightFixture;
        _appFixture = appFixture;
        _output = output;
    }

    public async Task InitializeAsync()
    {
        await _appFixture.Factory.ResetDatabaseAsync();

        _output.WriteLine($"Test server running at: {_appFixture.BaseUrl}");

        _context = await _playwrightFixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddPersonToGroup_ViaPersonsPage_GroupCountUpdatesInTable()
    {
        // Arrange
        await _appFixture.Factory.SeedPersonAsync("Alice");
        await _appFixture.Factory.SeedGroupAsync("Koor");

        await _page.GotoAsync($"{_appFixture.BaseUrl}/personen");
        await WaitForTableLoaded();

        var personRow = _page.Locator("table tbody tr:has-text('Alice')");
        await personRow.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });

        var groupsButton = personRow.Locator("[data-testid='groups-btn']");
        await groupsButton.ClickAsync();
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });

        // Act
        await _page.SelectAllOptionsAsync("person-available-groups");
        await _page.Locator("[data-testid='move-right-btn']").ClickAsync();
        await _page.WaitForTimeoutAsync(500);

        await _page.AssertNoOptionsSelectedAsync("person-available-groups");

        var closeButton = _page.Locator(".modal.show button:has-text('Sluiten')");
        await closeButton.ClickAsync();
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = 5000 });

        // Assert
        var updatedPersonRow = _page.Locator("table tbody tr:has-text('Alice')");
        var groupCountCell = updatedPersonRow.Locator("[data-testid='group-count']");
        await Assertions.Expect(groupCountCell).ToHaveTextAsync("1", new LocatorAssertionsToHaveTextOptions { Timeout = 5000 });
    }

    private async Task WaitForTableLoaded()
    {
        await _page.WaitForSelectorAsync("table.table", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await _page.WaitForFunctionAsync("() => document.querySelectorAll('table tbody .placeholder').length === 0", null, new PageWaitForFunctionOptions { Timeout = 10000 });
    }
}
