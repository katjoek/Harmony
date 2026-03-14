namespace Harmony.Tests.Web.Services;

using Harmony.ApplicationCore.DTOs;
using Harmony.Web.Models;
using Harmony.Web.Services;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using OfficeOpenXml;
using Xunit;

public sealed class ReportServiceTests
{
    private readonly ReportService _service = new();

    // -- Group report: PDF --

    [Fact]
    public async Task GeneratePdfReport_WithGroupAndMembers_ReturnsNonEmptyBytes()
    {
        // Act
        var result = await _service.GeneratePdfReportAsync(CreateGroup(), CreateMembers(), DefaultConfig());

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GeneratePdfReport_WithoutCoordinator_ReturnsNonEmptyBytes()
    {
        // Act
        var result = await _service.GeneratePdfReportAsync(CreateGroup(coordinatorName: null), CreateMembers(), DefaultConfig());

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GeneratePdfReport_WithEmptyMemberList_ReturnsNonEmptyBytes()
    {
        // Act
        var result = await _service.GeneratePdfReportAsync(CreateGroup(), [], DefaultConfig());

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GeneratePdfReport_WithNoOptionalColumns_ReturnsNonEmptyBytes()
    {
        // Act
        var result = await _service.GeneratePdfReportAsync(
            CreateGroup(), CreateMembers(),
            DefaultConfig(includeDateOfBirth: false, includeAddress: false, includePhoneNumber: false, includeEmailAddress: false));

        // Assert
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData("Portrait")]
    [InlineData("Landscape")]
    public async Task GeneratePdfReport_WithOrientation_ReturnsNonEmptyBytes(string orientation)
    {
        // Act
        var result = await _service.GeneratePdfReportAsync(CreateGroup(), CreateMembers(), DefaultConfig(orientation: orientation));

        // Assert
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData("FirstName")]
    [InlineData("LastName")]
    public async Task GeneratePdfReport_WithSortOrder_ReturnsNonEmptyBytes(string sortOrder)
    {
        // Act
        var result = await _service.GeneratePdfReportAsync(CreateGroup(), CreateMembers(), DefaultConfig(sortOrder: sortOrder));

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GeneratePdfReport_WithMembersLackingOptionalData_ReturnsNonEmptyBytes()
    {
        // Arrange — persons with no optional fields set
        var members = new List<PersonDto>
        {
            new("id-1", "Anna", null, null, null, null, null, null, null, null, []),
            new("id-2", "Zoe",  null, null, null, null, null, null, null, null, [])
        };

        // Act
        var result = await _service.GeneratePdfReportAsync(CreateGroup(), members, DefaultConfig());

        // Assert
        Assert.NotEmpty(result);
    }

    // -- Group report: Excel --

    [Fact]
    public async Task GenerateExcelReport_WithGroupAndMembers_ReturnsNonEmptyBytes()
    {
        // Act
        var result = await _service.GenerateExcelReportAsync(CreateGroup(), CreateMembers(), DefaultConfig());

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GenerateExcelReport_WithoutCoordinator_ReturnsNonEmptyBytes()
    {
        // Act
        var result = await _service.GenerateExcelReportAsync(CreateGroup(coordinatorName: null), CreateMembers(), DefaultConfig());

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GenerateExcelReport_WithEmptyMemberList_ReturnsNonEmptyBytes()
    {
        // Act
        var result = await _service.GenerateExcelReportAsync(CreateGroup(), [], DefaultConfig());

        // Assert
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData("Portrait")]
    [InlineData("Landscape")]
    public async Task GenerateExcelReport_WithOrientation_ReturnsNonEmptyBytes(string orientation)
    {
        // Act
        var result = await _service.GenerateExcelReportAsync(CreateGroup(), CreateMembers(), DefaultConfig(orientation: orientation));

        // Assert
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData("FirstName")]
    [InlineData("LastName")]
    public async Task GenerateExcelReport_WithSortOrder_ReturnsNonEmptyBytes(string sortOrder)
    {
        // Act
        var result = await _service.GenerateExcelReportAsync(CreateGroup(), CreateMembers(), DefaultConfig(sortOrder: sortOrder));

        // Assert
        Assert.NotEmpty(result);
    }

    // -- Birthday report: PDF --

    [Fact]
    public async Task GenerateBirthdayPdfReport_WithPersons_ReturnsNonEmptyBytes()
    {
        // Act
        var result = await _service.GenerateBirthdayPdfReportAsync("januari", CreateMembers(), DefaultConfig());

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GenerateBirthdayPdfReport_WithEmptyPersonList_ReturnsNonEmptyBytes()
    {
        // Act
        var result = await _service.GenerateBirthdayPdfReportAsync("januari", [], DefaultConfig());

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GenerateBirthdayPdfReport_WithPersonsWithoutDateOfBirth_ReturnsNonEmptyBytes()
    {
        // Arrange — persons with null DateOfBirth
        var persons = new List<PersonDto>
        {
            new("id-1", "Anna", null, "Bakker", null, null, null, null, null, null, []),
            new("id-2", "Zoe",  null, "Adam",   null, null, null, null, null, null, [])
        };

        // Act
        var result = await _service.GenerateBirthdayPdfReportAsync("januari", persons, DefaultConfig());

        // Assert
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData("FirstName")]
    [InlineData("LastName")]
    public async Task GenerateBirthdayPdfReport_WithSortOrder_ReturnsNonEmptyBytes(string sortOrder)
    {
        // Act
        var result = await _service.GenerateBirthdayPdfReportAsync("maart", CreateMembers(), DefaultConfig(sortOrder: sortOrder));

        // Assert
        Assert.NotEmpty(result);
    }

    // -- Birthday report: Excel --

    [Fact]
    public async Task GenerateBirthdayExcelReport_WithPersons_ReturnsNonEmptyBytes()
    {
        // Act
        var result = await _service.GenerateBirthdayExcelReportAsync("januari", CreateMembers(), DefaultConfig());

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GenerateBirthdayExcelReport_WithEmptyPersonList_ReturnsNonEmptyBytes()
    {
        // Act
        var result = await _service.GenerateBirthdayExcelReportAsync("januari", [], DefaultConfig());

        // Assert
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData("Portrait")]
    [InlineData("Landscape")]
    public async Task GenerateBirthdayExcelReport_WithOrientation_ReturnsNonEmptyBytes(string orientation)
    {
        // Act
        var result = await _service.GenerateBirthdayExcelReportAsync("maart", CreateMembers(), DefaultConfig(orientation: orientation));

        // Assert
        Assert.NotEmpty(result);
    }

    // -- Content verification: PDF --

    [Fact]
    public async Task GeneratePdfReport_WithGroupAndMembers_PdfContainsGroupNameMemberNamesAndCoordinator()
    {
        // Act
        var bytes = await _service.GeneratePdfReportAsync(CreateGroup(), CreateMembers(), DefaultConfig());

        // Assert
        var text = ExtractPdfText(bytes);
        Assert.Contains("Bijbelkring Noord", text);
        Assert.Contains("Piet Janssen", text);
        Assert.Contains("Anna Bakker", text);
        Assert.Contains("Zoe Adam", text);
        Assert.Contains("Bert van Dijk", text);
    }

    [Fact]
    public async Task GeneratePdfReport_WithLastNameSort_MembersAppearInAlphabeticalSurnameOrder()
    {
        // Arrange — Adam < Bakker < Dijk
        // Act
        var bytes = await _service.GeneratePdfReportAsync(CreateGroup(), CreateMembers(), DefaultConfig(sortOrder: "LastName"));

        // Assert
        var text = ExtractPdfText(bytes);
        Assert.True(text.IndexOf("Adam",   StringComparison.Ordinal) < text.IndexOf("Bakker", StringComparison.Ordinal));
        Assert.True(text.IndexOf("Bakker", StringComparison.Ordinal) < text.IndexOf("Dijk",   StringComparison.Ordinal));
    }

    // -- Content verification: Excel --

    [Fact]
    public async Task GenerateExcelReport_WithGroupAndMembers_WorksheetContainsGroupNameMemberNamesAndCoordinator()
    {
        // Act
        var bytes = await _service.GenerateExcelReportAsync(CreateGroup(), CreateMembers(), DefaultConfig());

        // Assert
        ExcelPackage.License.SetNonCommercialPersonal("Harmony");
        using var package = new ExcelPackage(new MemoryStream(bytes));
        var values = AllCellValues(package.Workbook.Worksheets[0]).ToList();
        Assert.Contains("Groepsrapport: Bijbelkring Noord", values);
        Assert.Contains("Coördinator: Piet Janssen", values);
        Assert.Contains("Anna Bakker", values);
        Assert.Contains("Zoe Adam", values);
        Assert.Contains("Bert van Dijk", values);
    }

    [Theory]
    [InlineData("LastName",  "Zoe Adam")]    // Adam < Bakker < Dijk
    [InlineData("FirstName", "Anna Bakker")] // Anna < Bert < Zoe
    public async Task GenerateExcelReport_SortOrderDeterminesFirstDataRow(string sortOrder, string expectedFirstMember)
    {
        // Act
        var bytes = await _service.GenerateExcelReportAsync(CreateGroup(), CreateMembers(), DefaultConfig(sortOrder: sortOrder));

        // Assert — row layout: 1=title, 3=timestamp, 4=coordinator, 7=headers, 8=first data row
        ExcelPackage.License.SetNonCommercialPersonal("Harmony");
        using var package = new ExcelPackage(new MemoryStream(bytes));
        var firstDataRowName = package.Workbook.Worksheets[0].Cells[8, 1].Value?.ToString();
        Assert.Equal(expectedFirstMember, firstDataRowName);
    }

    // -- Helpers --

    private static string ExtractPdfText(byte[] pdfBytes)
    {
        using var stream = new MemoryStream(pdfBytes);
        using var reader = new PdfReader(stream);
        using var pdf = new PdfDocument(reader);
        var text = new System.Text.StringBuilder();
        for (var i = 1; i <= pdf.GetNumberOfPages(); i++)
            text.Append(PdfTextExtractor.GetTextFromPage(pdf.GetPage(i)));
        return text.ToString();
    }

    private static IEnumerable<string> AllCellValues(ExcelWorksheet worksheet)
    {
        for (var row = 1; row <= worksheet.Dimension.End.Row; row++)
            for (var col = 1; col <= worksheet.Dimension.End.Column; col++)
                if (worksheet.Cells[row, col].Value?.ToString() is { Length: > 0 } value)
                    yield return value;
    }

    // -- Test data factory methods --

    private static GroupDto CreateGroup(string? coordinatorName = "Piet Janssen") =>
        new("group-1", "Bijbelkring Noord", "coord-1", coordinatorName, [], 0);

    private static List<PersonDto> CreateMembers() =>
    [
        new("id-1", "Anna",  null,  "Bakker", new DateOnly(1985,  3, 12), "Kerkstraat 1", "1234AB", "Amsterdam", "06-11111111", "anna@example.com", []),
        new("id-2", "Zoe",   null,  "Adam",   new DateOnly(1990,  7,  4), "Marktplein 5", "5678CD", "Rotterdam", "06-22222222", "zoe@example.com",  []),
        new("id-3", "Bert",  "van", "Dijk",   new DateOnly(1978, 11, 30), "Dorpsweg 10",  "9012EF", "Utrecht",   "06-33333333", "bert@example.com", [])
    ];

    private static ReportModel DefaultConfig(
        string orientation = "Portrait",
        string sortOrder = "LastName",
        bool includeDateOfBirth = true,
        bool includeAddress = true,
        bool includePhoneNumber = true,
        bool includeEmailAddress = true) => new()
    {
        IncludeFullName = true,
        IncludeDateOfBirth = includeDateOfBirth,
        IncludeAddress = includeAddress,
        IncludePhoneNumber = includePhoneNumber,
        IncludeEmailAddress = includeEmailAddress,
        OutputFormat = "PDF",
        Orientation = orientation,
        SortOrder = sortOrder
    };
}
