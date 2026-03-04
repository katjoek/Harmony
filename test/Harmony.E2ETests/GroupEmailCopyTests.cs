namespace Harmony.E2ETests;

using Harmony.E2ETests.Infrastructure;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

[Collection(nameof(PlaywrightCollection))]
public sealed class GroupEmailCopyTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _playwrightFixture;
    private readonly ITestOutputHelper _output;
    private HarmonyWebApplicationFactory _factory = null!;
    private IBrowserContext _context = null!;
    private IPage _page = null!;
    private string _baseUrl = null!;

    public GroupEmailCopyTests(PlaywrightFixture playwrightFixture, ITestOutputHelper output)
    {
        _playwrightFixture = playwrightFixture;
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _factory = new HarmonyWebApplicationFactory();
        _ = _factory.CreateClient();
        await _factory.InitializeDatabaseAsync();
        _baseUrl = _factory.BaseUrl;
        _output.WriteLine($"Test server started at: {_baseUrl}");

        _context = await _playwrightFixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            Permissions = new[] { "clipboard-read", "clipboard-write" }
        });
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
        await _context.DisposeAsync();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task CopyEmails_GroupWithMembersWithEmail_CopiesEmailsToClipboard()
    {
        var email1 = "alice@example.com";
        var email2 = "bob@example.com";
        var groupName = "TestGroep";

        await CreatePerson("Alice", email1);
        await CreatePerson("Bob", email2);
        await CreatePerson("Charlie", null);

        await CreateGroup(groupName);

        await AddAllPersonsToGroup(groupName);

        await _page.GotoAsync($"{_baseUrl}/groepen");
        await WaitForTableLoaded();

        var groupRow = _page.Locator($"table tbody tr:has-text('{groupName}')");
        await groupRow.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });

        var copyButton = groupRow.Locator("[data-testid='copy-emails']");
        await copyButton.ClickAsync();
        _output.WriteLine("Clicked email copy button");

        var clipboardText = await _page.EvaluateAsync<string>("navigator.clipboard.readText()");
        _output.WriteLine($"Clipboard content: {clipboardText}");

        Assert.Contains(email1, clipboardText);
        Assert.Contains(email2, clipboardText);
        Assert.DoesNotContain("Charlie", clipboardText);
    }

    [Fact]
    public async Task CopyEmails_GroupWithNoEmailAddresses_ShowsAlert()
    {
        var groupName = "LegeMailGroep";

        await CreatePerson("Dirk", null);
        await CreateGroup(groupName);
        await AddAllPersonsToGroup(groupName);

        await _page.GotoAsync($"{_baseUrl}/groepen");
        await WaitForTableLoaded();

        var groupRow = _page.Locator($"table tbody tr:has-text('{groupName}')");
        await groupRow.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });

        string? dialogMessage = null;
        _page.Dialog += async (_, dialog) =>
        {
            dialogMessage = dialog.Message;
            await dialog.AcceptAsync();
        };

        var copyButton = groupRow.Locator("[data-testid='copy-emails']");
        await copyButton.ClickAsync();
        await _page.WaitForTimeoutAsync(1000);

        _output.WriteLine($"Alert message: {dialogMessage}");
        Assert.NotNull(dialogMessage);
        Assert.Contains("geen e-mailadressen", dialogMessage, StringComparison.OrdinalIgnoreCase);
    }

    private async Task CreatePerson(string firstName, string? email)
    {
        await _page.GotoAsync($"{_baseUrl}/personen");
        await _page.WaitForSelectorAsync("table.table", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });

        await _page.ClickAsync("[data-testid='new-person-btn']");
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });

        await _page.FillAsync("[data-testid='input-firstname']", firstName);

        if (!string.IsNullOrEmpty(email))
        {
            await _page.FillAsync("[data-testid='input-email']", email);
        }

        await _page.ClickAsync("[data-testid='modal-save-btn']");
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = 5000 });
        _output.WriteLine($"Created person: {firstName}");
    }

    private async Task CreateGroup(string groupName)
    {
        await _page.GotoAsync($"{_baseUrl}/groepen");
        await WaitForTableLoaded();

        await _page.ClickAsync("[data-testid='new-group-btn']");
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });

        await _page.FillAsync("[data-testid='input-groupname']", groupName);

        await _page.ClickAsync("[data-testid='modal-save-btn']");
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = 5000 });
        _output.WriteLine($"Created group: {groupName}");
    }

    private async Task AddAllPersonsToGroup(string groupName)
    {
        await _page.GotoAsync($"{_baseUrl}/groepen");
        await WaitForTableLoaded();

        var groupRow = _page.Locator($"table tbody tr:has-text('{groupName}')");
        await groupRow.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });

        var membersButton = groupRow.Locator("button.btn-info");
        await membersButton.ClickAsync();
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });

        var selectAll = _page.Locator("#group-available");
        await selectAll.EvaluateAsync("el => { for (let o of el.options) o.selected = true; el.dispatchEvent(new Event('change', { bubbles: true })); }");

        var moveRightButton = _page.Locator("button[title='Naar rechts']");
        await moveRightButton.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await moveRightButton.ClickAsync();

        var closeButton = _page.Locator(".modal.show button:has-text('Sluiten')");
        await closeButton.ClickAsync();
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { State = WaitForSelectorState.Hidden, Timeout = 5000 });
        _output.WriteLine($"Added all persons to group: {groupName}");
    }

    private async Task WaitForTableLoaded()
    {
        await _page.WaitForSelectorAsync("table.table", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });
        await _page.WaitForFunctionAsync("() => document.querySelectorAll('table tbody .placeholder').length === 0", null, new PageWaitForFunctionOptions { Timeout = 10000 });
    }
}
