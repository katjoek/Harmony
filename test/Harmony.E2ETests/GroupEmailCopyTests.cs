namespace Harmony.E2ETests;

using Harmony.E2ETests.Infrastructure;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

[Collection(nameof(PlaywrightCollection))]
public sealed class GroupEmailCopyTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _playwrightFixture;
    private readonly HarmonyAppFixture _appFixture;
    private readonly ITestOutputHelper _output;
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    public GroupEmailCopyTests(PlaywrightFixture playwrightFixture, HarmonyAppFixture appFixture, ITestOutputHelper output)
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
            IgnoreHTTPSErrors = true,
            Permissions = new[] { "clipboard-read", "clipboard-write" }
        });
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task CopyEmails_GroupWithMembersWithEmail_CopiesEmailsToClipboard()
    {
        var email1 = "alice@example.com";
        var email2 = "bob@example.com";
        var groupName = "TestGroep";

        await _appFixture.Factory.SeedPersonAsync("Alice", email1);
        await _appFixture.Factory.SeedPersonAsync("Bob", email2);
        await _appFixture.Factory.SeedPersonAsync("Charlie");

        await _appFixture.Factory.SeedGroupAsync(groupName);

        await AddAllPersonsToGroup(groupName);

        await _page.GotoAsync($"{_appFixture.BaseUrl}/groepen");
        await WaitForTableLoaded();

        var groupRow = _page.Locator($"table tbody tr:has-text('{groupName}')");
        await groupRow.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10000 });

        var copyButton = groupRow.Locator("[data-testid='copy-emails']");
        await copyButton.ClickAsync();
        _output.WriteLine("Clicked email copy button");

        var clipboardText = await _page.EvaluateAsync<string>(@"async () => {
            for (let i = 0; i < 20; i++) {
                const text = await navigator.clipboard.readText();
                if (text) return text;
                await new Promise(r => setTimeout(r, 100));
            }
            return await navigator.clipboard.readText();
        }");
        _output.WriteLine($"Clipboard content: {clipboardText}");

        Assert.Contains(email1, clipboardText);
        Assert.Contains(email2, clipboardText);
        Assert.DoesNotContain("Charlie", clipboardText);
    }

    [Fact]
    public async Task CopyEmails_GroupWithNoEmailAddresses_ShowsAlert()
    {
        var groupName = "LegeMailGroep";

        await _appFixture.Factory.SeedPersonAsync("Dirk");
        await _appFixture.Factory.SeedGroupAsync(groupName);
        await AddAllPersonsToGroup(groupName);

        await _page.GotoAsync($"{_appFixture.BaseUrl}/groepen");
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

    private async Task AddAllPersonsToGroup(string groupName)
    {
        await _page.GotoAsync($"{_appFixture.BaseUrl}/groepen");
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
