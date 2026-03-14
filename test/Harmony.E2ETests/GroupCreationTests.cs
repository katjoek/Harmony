namespace Harmony.E2ETests;

using Harmony.E2ETests.Infrastructure;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

[Collection(nameof(PlaywrightCollection))]
public sealed class GroupCreationTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _playwrightFixture;
    private readonly HarmonyAppFixture _appFixture;
    private readonly ITestOutputHelper _output;
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    public GroupCreationTests(PlaywrightFixture playwrightFixture, HarmonyAppFixture appFixture, ITestOutputHelper output)
    {
        _playwrightFixture = playwrightFixture;
        _appFixture = appFixture;
        _output = output;
    }

    public async Task InitializeAsync()
    {
        await _appFixture.Factory.ResetDatabaseAsync();

        _output.WriteLine($"Test server running at: {_appFixture.BaseUrl}");

        _context = await _playwrightFixture.CreateContextAsync();
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task CreateGroup_WithValidName_GroupAppearsInList()
    {
        // Arrange
        var groupName = "Testgroep";

        // Act - Navigate to groups page
        await _page.GotoAsync($"{_appFixture.BaseUrl}/groepen");

        await _page.WaitForSelectorAsync("table.table", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 10000
        });

        await _page.ClickAsync("[data-testid='new-group-btn']");

        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 5000
        });

        await _page.FillAsync("[data-testid='input-groupname']", groupName);
        await _page.ClickAsync("[data-testid='modal-save-btn']");

        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = 5000
        });

        // Assert
        var groupRow = _page.Locator($"table tbody tr:has-text('{groupName}')");
        await groupRow.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 10000
        });

        var rowText = await groupRow.TextContentAsync();
        _output.WriteLine($"Found group row: {rowText}");
        Assert.Contains(groupName, rowText);
    }

    [Fact]
    public async Task CreateGroup_WithoutName_ShowsValidationError()
    {
        // Act - Navigate to groups page
        await _page.GotoAsync($"{_appFixture.BaseUrl}/groepen");

        await _page.WaitForSelectorAsync("table.table", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 10000
        });

        await _page.ClickAsync("[data-testid='new-group-btn']");

        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 5000
        });

        // Submit without filling in the name
        await _page.ClickAsync("[data-testid='modal-save-btn']");

        // Assert - modal stays open
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 2000
        });

        // Assert - validation message is shown
        var validationMessage = _page.Locator(".validation-message");
        await validationMessage.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 2000
        });

        var messageText = await validationMessage.TextContentAsync();
        _output.WriteLine($"Validation message: {messageText}");
        Assert.Contains("Groepsnaam is verplicht", messageText);
    }
}
