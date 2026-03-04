namespace Harmony.E2ETests;

using Harmony.E2ETests.Infrastructure;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

[Collection(nameof(PlaywrightCollection))]
public sealed class PersonCreationTests : IAsyncLifetime
{
    private readonly PlaywrightFixture _playwrightFixture;
    private readonly ITestOutputHelper _output;
    private HarmonyWebApplicationFactory _factory = null!;
    private IBrowserContext _context = null!;
    private IPage _page = null!;
    private string _baseUrl = null!;

    public PersonCreationTests(PlaywrightFixture playwrightFixture, ITestOutputHelper output)
    {
        _playwrightFixture = playwrightFixture;
        _output = output;
    }

    public async Task InitializeAsync()
    {
        // Create a new factory with isolated database for each test
        _factory = new HarmonyWebApplicationFactory();
        
        // Start the server (this creates the host)
        _ = _factory.CreateClient();
        
        // Initialize the test database
        await _factory.InitializeDatabaseAsync();
        
        // Get the URL from the factory
        _baseUrl = _factory.BaseUrl;
        _output.WriteLine($"Test server started at: {_baseUrl}");
        
        // Create isolated browser context for this test
        _context = await _playwrightFixture.CreateContextAsync();
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
        await _context.DisposeAsync();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task CreatePerson_WithValidData_PersonAppearsInList()
    {
        // Arrange
        var firstName = "Jan";
        var prefix = "van";
        var surname = "Berg";
        var dateOfBirthHtmlFormat = "1990-05-15"; // HTML date input format
        var street = "Hoofdstraat 1";
        var zipCode = "1234AB";
        var city = "Amsterdam";
        var phoneNumber = "06-12345678";
        var email = "jan@example.com";

        // Act - Navigate to persons page
        await _page.GotoAsync($"{_baseUrl}/personen");
        _output.WriteLine("Navigated to persons page");

        // Wait for the page to load (skeleton placeholders should disappear)
        await _page.WaitForSelectorAsync("table.table", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 10000
        });

        // Click "Nieuwe Persoon" button to open the modal
        await _page.ClickAsync("[data-testid='new-person-btn']");
        _output.WriteLine("Clicked 'Nieuwe Persoon' button");

        // Wait for the modal to appear
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 5000
        });
        _output.WriteLine("Modal is visible");

        // Fill in the form fields
        await _page.FillAsync("[data-testid='input-firstname']", firstName);
        await _page.FillAsync("[data-testid='input-prefix']", prefix);
        await _page.FillAsync("[data-testid='input-surname']", surname);
        await _page.FillAsync("[data-testid='input-dateofbirth']", dateOfBirthHtmlFormat);
        await _page.FillAsync("[data-testid='input-phonenumber']", phoneNumber);
        await _page.FillAsync("[data-testid='input-email']", email);
        await _page.FillAsync("[data-testid='input-street']", street);
        await _page.FillAsync("[data-testid='input-zipcode']", zipCode);
        await _page.FillAsync("[data-testid='input-city']", city);

        _output.WriteLine("Form fields filled");

        // Click the "Opslaan" (Save) button
        await _page.ClickAsync("[data-testid='modal-save-btn']");
        _output.WriteLine("Clicked 'Opslaan' button");

        // Wait for the modal to close
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = 5000
        });
        _output.WriteLine("Modal closed");

        // Assert - Wait for the table to update and verify the person is in the list
        await _page.WaitForSelectorAsync($"table tbody tr:has-text('{firstName}')", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 10000
        });

        // Verify the full name is displayed
        var personRow = _page.Locator($"table tbody tr:has-text('{firstName}')");
        await personRow.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

        var personRowText = await personRow.TextContentAsync();
        _output.WriteLine($"Found person row: {personRowText}");

        // Assert the row contains expected values
        Assert.Contains(firstName, personRowText);
        Assert.Contains(city, personRowText);
        Assert.Contains(email, personRowText);
    }

    [Fact]
    public async Task CreatePerson_WithMinimalData_PersonAppearsInList()
    {
        // Arrange - Only first name is required
        var firstName = "Piet";

        // Act - Navigate to persons page
        await _page.GotoAsync($"{_baseUrl}/personen");

        // Wait for the page to load
        await _page.WaitForSelectorAsync("table.table", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 10000
        });

        // Click "Nieuwe Persoon" button
        await _page.ClickAsync("[data-testid='new-person-btn']");

        // Wait for the modal
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 5000
        });

        // Fill only the required first name field
        await _page.FillAsync("[data-testid='input-firstname']", firstName);

        // Submit the form
        await _page.ClickAsync("[data-testid='modal-save-btn']");

        // Wait for modal to close
        await _page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = 5000
        });

        // Assert - Verify person is in the list
        var personRow = _page.Locator($"table tbody tr:has-text('{firstName}')");
        await personRow.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 10000
        });

        var personRowText = await personRow.TextContentAsync();
        Assert.Contains(firstName, personRowText);
    }
}
