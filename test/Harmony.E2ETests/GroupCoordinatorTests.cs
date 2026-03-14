namespace Harmony.E2ETests;

using Harmony.E2ETests.Infrastructure;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

[Collection(nameof(PlaywrightCollection))]
public sealed class GroupCoordinatorTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _playwrightFixture;
    private readonly HarmonyAppFixture _appFixture;
    private readonly ITestOutputHelper _output;
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    public GroupCoordinatorTests(PlaywrightFixture playwrightFixture, HarmonyAppFixture appFixture, ITestOutputHelper output)
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
    public async Task SetCoordinator_ToGroupMember_CoordinatorNameAppearsInTable()
    {
        // Arrange
        await _appFixture.Factory.SeedPersonAsync("Alice");
        await _appFixture.Factory.SeedGroupAsync("Koor");
        await _appFixture.Factory.SeedMembershipAsync("Alice", "Koor");

        await _page.GotoAsync($"{_appFixture.BaseUrl}/groepen");
        await WaitForTableLoaded();

        // Act
        await OpenEditModal("Koor");
        await _page.SelectOptionAsync("[data-testid='input-coordinator']", new SelectOptionValue { Label = "Alice" });
        await _page.Locator("[data-testid='modal-save-btn']").ClickAsync();
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = 5000 });

        // Assert
        var groupRow = _page.Locator("table tbody tr:has-text('Koor')");
        var coordinatorCell = groupRow.Locator("[data-testid='coordinator-name']");
        await Assertions.Expect(coordinatorCell).ToContainTextAsync("Alice", new LocatorAssertionsToContainTextOptions { Timeout = 5000 });
    }

    [Fact]
    public async Task CoordinatorDropdown_OnlyShowsGroupMembers_NonMemberNotAvailable()
    {
        // Arrange
        await _appFixture.Factory.SeedPersonAsync("Alice");
        await _appFixture.Factory.SeedPersonAsync("Bob");
        await _appFixture.Factory.SeedGroupAsync("Koor");
        await _appFixture.Factory.SeedMembershipAsync("Alice", "Koor");

        await _page.GotoAsync($"{_appFixture.BaseUrl}/groepen");
        await WaitForTableLoaded();

        // Act
        await OpenEditModal("Koor");

        // Assert
        var optionTexts = await _page.EvaluateAsync<string[]>(
            "() => Array.from(document.querySelector('[data-testid=\"input-coordinator\"]').options).map(o => o.text)");
        Assert.Contains(optionTexts, o => o.Contains("Alice"));
        Assert.DoesNotContain(optionTexts, o => o.Contains("Bob"));

        await _page.Locator(".modal.show button:has-text('Annuleren')").ClickAsync();
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = 5000 });
    }

    [Fact]
    public async Task ClearCoordinator_CoordinatorCellBecomesEmpty()
    {
        // Arrange
        await _appFixture.Factory.SeedPersonAsync("Alice");
        await _appFixture.Factory.SeedGroupAsync("Koor");
        await _appFixture.Factory.SeedMembershipAsync("Alice", "Koor");
        await _appFixture.Factory.SeedCoordinatorAsync("Alice", "Koor");

        await _page.GotoAsync($"{_appFixture.BaseUrl}/groepen");
        await WaitForTableLoaded();

        // Act
        await OpenEditModal("Koor");
        await _page.SelectOptionAsync("[data-testid='input-coordinator']", new SelectOptionValue { Value = "" });
        await _page.Locator("[data-testid='modal-save-btn']").ClickAsync();
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = 5000 });

        // Assert
        var groupRow = _page.Locator("table tbody tr:has-text('Koor')");
        var coordinatorCell = groupRow.Locator("[data-testid='coordinator-name']");
        await Assertions.Expect(coordinatorCell).ToHaveTextAsync("", new LocatorAssertionsToHaveTextOptions { Timeout = 5000 });
    }

    private async Task OpenEditModal(string groupName)
    {
        var groupRow = _page.Locator($"table tbody tr:has-text('{groupName}')");
        await groupRow.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });

        var editButton = groupRow.Locator("[data-testid='edit-group-btn']");
        await editButton.ClickAsync();
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }

    private async Task WaitForTableLoaded()
    {
        await _page.WaitForSelectorAsync("table.table", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await _page.WaitForFunctionAsync("() => document.querySelectorAll('table tbody .placeholder').length === 0", null, new PageWaitForFunctionOptions { Timeout = 10000 });
    }
}
