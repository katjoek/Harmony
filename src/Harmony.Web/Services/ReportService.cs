namespace Harmony.Web.Services;

using Harmony.ApplicationCore.DTOs;
using Harmony.Web.Models;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using OfficeOpenXml;
using OfficeOpenXml.Style;

public sealed class ReportService : IReportService
{
    private sealed record ReportColumn(string Header, Func<PersonDto, string> GetValue);

    public Task<byte[]> GeneratePdfReportAsync(GroupDto group, List<PersonDto> members, ReportModel config)
    {
        var subtitle = string.IsNullOrEmpty(group.CoordinatorName) ? null : $"Coördinator: {group.CoordinatorName}";
        return Task.FromResult(RenderPdf(
            $"Groepsrapport: {group.Name}",
            subtitle,
            BuildGroupColumns(config),
            SortForGroupReport(members, config),
            config));
    }

    public Task<byte[]> GenerateExcelReportAsync(GroupDto group, List<PersonDto> members, ReportModel config)
    {
        var subtitle = string.IsNullOrEmpty(group.CoordinatorName) ? null : $"Coördinator: {group.CoordinatorName}";
        return Task.FromResult(RenderExcel(
            $"Rapport {group.Name}",
            $"Groepsrapport: {group.Name}",
            subtitle,
            BuildGroupColumns(config),
            SortForGroupReport(members, config),
            config));
    }

    public Task<byte[]> GenerateBirthdayPdfReportAsync(string monthNameNl, List<PersonDto> persons, ReportModel config) =>
        Task.FromResult(RenderPdf(
            $"Verjaardagen in {monthNameNl}",
            null,
            BuildBirthdayColumns(config),
            SortForBirthdayReport(persons, config),
            config));

    public Task<byte[]> GenerateBirthdayExcelReportAsync(string monthNameNl, List<PersonDto> persons, ReportModel config) =>
        Task.FromResult(RenderExcel(
            $"Verjaardagen {monthNameNl}",
            $"Verjaardagen in {monthNameNl}",
            null,
            BuildBirthdayColumns(config),
            SortForBirthdayReport(persons, config),
            config));

    private static byte[] RenderPdf(
        string title,
        string? subtitle,
        IReadOnlyList<ReportColumn> columns,
        IReadOnlyList<PersonDto> rows,
        ReportModel config)
    {
        using var stream = new MemoryStream();
        using var writer = new PdfWriter(stream);
        using var pdf = new PdfDocument(writer);
        pdf.SetDefaultPageSize(ResolvePageSize(config));
        using var document = new Document(pdf);

        document.Add(new Paragraph(title)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(20)
            .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)));

        document.Add(new Paragraph($"Gegenereerd op: {DateTime.Now:dd-MM-yyyy HH:mm}")
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(10)
            .SetFontColor(ColorConstants.GRAY));

        if (!string.IsNullOrEmpty(subtitle))
            document.Add(new Paragraph(subtitle)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(12)
                .SetMarginTop(10));

        document.Add(new Paragraph("\n"));

        var table = new Table(columns.Count);
        table.SetWidth(UnitValue.CreatePercentValue(100));
        table.SetFontSize(9);

        foreach (var column in columns)
            AddPdfHeaderCell(table, column.Header);

        foreach (var row in rows)
            foreach (var column in columns)
                table.AddCell(new Cell().Add(new Paragraph(column.GetValue(row))));

        document.Add(table);
        document.Close();

        return stream.ToArray();
    }

    private static byte[] RenderExcel(
        string sheetName,
        string title,
        string? subtitle,
        IReadOnlyList<ReportColumn> columns,
        IReadOnlyList<PersonDto> rows,
        ReportModel config)
    {
        ExcelPackage.License.SetNonCommercialPersonal("Harmony");
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add(sheetName);

        worksheet.PrinterSettings.Orientation = IsLandscape(config)
            ? eOrientation.Landscape
            : eOrientation.Portrait;

        var row = 1;

        worksheet.Cells[row, 1].Value = title;
        worksheet.Cells[row, 1].Style.Font.Size = 16;
        worksheet.Cells[row, 1].Style.Font.Bold = true;
        row += 2;

        worksheet.Cells[row, 1].Value = $"Gegenereerd op: {DateTime.Now:dd-MM-yyyy HH:mm}";
        worksheet.Cells[row, 1].Style.Font.Size = 10;
        worksheet.Cells[row, 1].Style.Font.Color.SetColor(System.Drawing.Color.Gray);
        row++;

        if (!string.IsNullOrEmpty(subtitle))
        {
            worksheet.Cells[row, 1].Value = subtitle;
            worksheet.Cells[row, 1].Style.Font.Size = 12;
            row++;
        }

        row += 2;

        for (var col = 1; col <= columns.Count; col++)
            worksheet.Cells[row, col].Value = columns[col - 1].Header;

        using (var range = worksheet.Cells[row, 1, row, columns.Count])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }

        row++;

        foreach (var person in rows)
        {
            for (var col = 1; col <= columns.Count; col++)
                worksheet.Cells[row, col].Value = columns[col - 1].GetValue(person);

            using (var range = worksheet.Cells[row, 1, row, columns.Count])
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);

            row++;
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        return package.GetAsByteArray();
    }

    private static IReadOnlyList<ReportColumn> BuildGroupColumns(ReportModel config)
    {
        var columns = new List<ReportColumn> { new("Volledige naam", p => p.FullName ?? "") };
        if (config.IncludeDateOfBirth)  columns.Add(new("Geboortedatum", p => p.DateOfBirth?.ToString("dd-MM-yyyy") ?? ""));
        if (config.IncludeAddress)      columns.Add(new("Adres",         p => p.FormattedAddress ?? ""));
        if (config.IncludePhoneNumber)  columns.Add(new("Telefoon",      p => p.PhoneNumber ?? ""));
        if (config.IncludeEmailAddress) columns.Add(new("E-mail",        p => p.EmailAddress ?? ""));
        return columns;
    }

    private static IReadOnlyList<ReportColumn> BuildBirthdayColumns(ReportModel config)
    {
        var columns = new List<ReportColumn>();
        if (config.IncludeDateOfBirth)  columns.Add(new("Geboortedatum", p => p.DateOfBirth?.ToString("dd-MM-yyyy") ?? ""));
        columns.Add(new("Volledige naam", p => p.FullName ?? ""));
        if (config.IncludeAddress)      columns.Add(new("Adres",    p => p.FormattedAddress ?? ""));
        if (config.IncludePhoneNumber)  columns.Add(new("Telefoon", p => p.PhoneNumber ?? ""));
        if (config.IncludeEmailAddress) columns.Add(new("E-mail",   p => p.EmailAddress ?? ""));
        return columns;
    }

    private static IReadOnlyList<PersonDto> SortForGroupReport(IEnumerable<PersonDto> persons, ReportModel config) =>
        config.SortOrder == "FirstName"
            ? persons.OrderBy(p => p.FirstName).ThenBy(p => p.Surname ?? "").ToList()
            : persons.OrderBy(p => p.Surname ?? "").ThenBy(p => p.FirstName).ToList();

    private static IReadOnlyList<PersonDto> SortForBirthdayReport(IEnumerable<PersonDto> persons, ReportModel config) =>
        config.SortOrder == "FirstName"
            ? persons.OrderBy(p => p.DateOfBirth?.Day).ThenBy(p => p.FirstName).ThenBy(p => p.Surname ?? "").ToList()
            : persons.OrderBy(p => p.DateOfBirth?.Day).ThenBy(p => p.Surname ?? "").ThenBy(p => p.FirstName).ToList();

    private static PageSize ResolvePageSize(ReportModel config) =>
        IsLandscape(config) ? PageSize.A4.Rotate() : PageSize.A4;

    private static bool IsLandscape(ReportModel config) =>
        string.Equals(config.Orientation, "Landscape", StringComparison.OrdinalIgnoreCase);

    private static void AddPdfHeaderCell(Table table, string text)
    {
        var cell = new Cell().Add(new Paragraph(text));
        cell.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
        cell.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD));
        table.AddHeaderCell(cell);
    }
}
