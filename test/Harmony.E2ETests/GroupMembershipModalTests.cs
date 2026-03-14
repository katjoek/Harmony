namespace Harmony.E2ETests;

using Harmony.E2ETests.Infrastructure;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

[Collection(nameof(PlaywrightCollection))]
public sealed class GroupMembershipModalTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _playwrightFixture;
    private readonly HarmonyAppFixture _appFixture;
    private readonly ITestOutputHelper _output;
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    public GroupMembershipModalTests(PlaywrightFixture playwrightFixture, HarmonyAppFixture appFixture, ITestOutputHelper output)
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
    public async Task AddMember_ViaArrowButton_PersonAppearsInMembersSelect()
    {
        // Arrange
        await _appFixture.Factory.SeedPersonAsync("Alice");
        await _appFixture.Factory.SeedGroupAsync("Koor");

        await OpenMembersModal("Koor");

        // Act
        await _page.SelectAllOptionsAsync("group-available");
        await _page.Locator("[data-testid='move-right-btn']").ClickAsync();
        await _page.WaitForTimeoutAsync(500);

        // Assert
        var membersOptions = await _page.GetOptionTextsAsync("group-members");
        var availableOptions = await _page.GetOptionTextsAsync("group-available");
        Assert.Contains("Alice", membersOptions);
        Assert.DoesNotContain("Alice", availableOptions);
        await _page.AssertNoOptionsSelectedAsync("group-available");
    }

    [Fact]
    public async Task RemoveMember_ViaArrowButton_PersonAppearsInAvailableSelect()
    {
        // Arrange
        await _appFixture.Factory.SeedPersonAsync("Alice");
        await _appFixture.Factory.SeedGroupAsync("Koor");
        await _appFixture.Factory.SeedMembershipAsync("Alice", "Koor");

        await OpenMembersModal("Koor");

        // Act
        await _page.SelectAllOptionsAsync("group-members");
        await _page.Locator("[data-testid='move-left-btn']").ClickAsync();
        await _page.WaitForTimeoutAsync(500);

        // Assert
        var availableOptions = await _page.GetOptionTextsAsync("group-available");
        var membersOptions = await _page.GetOptionTextsAsync("group-members");
        Assert.Contains("Alice", availableOptions);
        Assert.DoesNotContain("Alice", membersOptions);
        await _page.AssertNoOptionsSelectedAsync("group-members");
    }

    [Fact]
    public async Task AddMember_ViaDoubleClick_PersonAppearsInMembersSelect()
    {
        // Arrange
        await _appFixture.Factory.SeedPersonAsync("Alice");
        await _appFixture.Factory.SeedGroupAsync("Koor");

        await OpenMembersModal("Koor");

        var optionValue = await _page.GetFirstOptionValueAsync("group-available");

        // Act
        await _page.DoubleClickOptionAsync("group-available", optionValue);
        await _page.WaitForTimeoutAsync(500);

        // Assert
        var membersOptions = await _page.GetOptionTextsAsync("group-members");
        var availableOptions = await _page.GetOptionTextsAsync("group-available");
        Assert.Contains("Alice", membersOptions);
        Assert.DoesNotContain("Alice", availableOptions);
        await _page.AssertNoOptionsSelectedAsync("group-available");
    }

    [Fact]
    public async Task RemoveMember_ViaDoubleClick_PersonAppearsInAvailableSelect()
    {
        // Arrange
        await _appFixture.Factory.SeedPersonAsync("Alice");
        await _appFixture.Factory.SeedGroupAsync("Koor");
        await _appFixture.Factory.SeedMembershipAsync("Alice", "Koor");

        await OpenMembersModal("Koor");

        var optionValue = await _page.GetFirstOptionValueAsync("group-members");

        // Act
        await _page.DoubleClickOptionAsync("group-members", optionValue);
        await _page.WaitForTimeoutAsync(500);

        // Assert
        var availableOptions = await _page.GetOptionTextsAsync("group-available");
        var membersOptions = await _page.GetOptionTextsAsync("group-members");
        Assert.Contains("Alice", availableOptions);
        Assert.DoesNotContain("Alice", membersOptions);
        await _page.AssertNoOptionsSelectedAsync("group-members");
    }

    [Fact]
    public async Task AddMultipleMembers_ViaArrowButton_AllPersonsAppearInMembersSelect()
    {
        // Arrange
        await _appFixture.Factory.SeedPersonAsync("Alice");
        await _appFixture.Factory.SeedPersonAsync("Bob");
        await _appFixture.Factory.SeedPersonAsync("Charlie");
        await _appFixture.Factory.SeedGroupAsync("Koor");

        await OpenMembersModal("Koor");

        // Act
        await _page.SelectAllOptionsAsync("group-available");
        await _page.Locator("[data-testid='move-right-btn']").ClickAsync();
        await _page.WaitForTimeoutAsync(500);

        // Assert
        var membersOptions = await _page.GetOptionTextsAsync("group-members");
        var availableOptions = await _page.GetOptionTextsAsync("group-available");
        Assert.Contains("Alice", membersOptions);
        Assert.Contains("Bob", membersOptions);
        Assert.Contains("Charlie", membersOptions);
        Assert.Empty(availableOptions);
        await _page.AssertNoOptionsSelectedAsync("group-available");
    }

    [Fact]
    public async Task CloseModal_ModalDismisses()
    {
        // Arrange
        await _appFixture.Factory.SeedGroupAsync("Koor");

        await OpenMembersModal("Koor");

        // Act
        var closeButton = _page.Locator(".modal.show button:has-text('Sluiten')");
        await closeButton.ClickAsync();

        // Assert
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = 5000 });
    }

    private async Task OpenMembersModal(string groupName)
    {
        await _page.GotoAsync($"{_appFixture.BaseUrl}/groepen");
        await WaitForTableLoaded();

        var groupRow = _page.Locator($"table tbody tr:has-text('{groupName}')");
        await groupRow.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });

        var membersButton = groupRow.Locator("[data-testid='members-btn']");
        await membersButton.ClickAsync();
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }

    private async Task WaitForTableLoaded()
    {
        await _page.WaitForSelectorAsync("table.table", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await _page.WaitForFunctionAsync("() => document.querySelectorAll('table tbody .placeholder').length === 0", null, new PageWaitForFunctionOptions { Timeout = 10000 });
    }
}
