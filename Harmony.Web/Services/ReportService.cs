using Harmony.ApplicationCore.DTOs;
using Harmony.Web.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Harmony.Web.Services;

public class ReportService : IReportService
{
    public Task<byte[]> GeneratePdfReportAsync(GroupDto group, List<PersonDto> members, ReportModel config)
    {
        try
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Title
            var title = new Paragraph($"Groepsrapport: {group.Name}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20)
                .SetBold();
            document.Add(title);

            // Timestamp
            var timestamp = new Paragraph($"Gegenereerd op: {DateTime.Now:dd-MM-yyyy HH:mm}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(10)
                .SetFontColor(ColorConstants.GRAY);
            document.Add(timestamp);

            // Coordinator if present
            if (!string.IsNullOrEmpty(group.CoordinatorName))
            {
                var coordinator = new Paragraph($"Coördinator: {group.CoordinatorName}")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12)
                    .SetMarginTop(10);
                document.Add(coordinator);
            }

            // Add some space
            document.Add(new Paragraph("\n"));

            // Create table
            var columnCount = GetColumnCount(config);
            var table = new Table(columnCount);
            table.SetWidth(UnitValue.CreatePercentValue(100));

            // Add headers
            AddTableHeader(table, "Volledige naam");
            if (config.IncludeDateOfBirth) AddTableHeader(table, "Geboortedatum");
            if (config.IncludeAddress) AddTableHeader(table, "Adres");
            if (config.IncludePhoneNumber) AddTableHeader(table, "Telefoon");
            if (config.IncludeEmailAddress) AddTableHeader(table, "E-mail");

            // Add member data
            foreach (var member in members)
            {
                table.AddCell(new Cell().Add(new Paragraph(member.FullName ?? "")));
                if (config.IncludeDateOfBirth) 
                    table.AddCell(new Cell().Add(new Paragraph(member.DateOfBirth?.ToString("dd-MM-yyyy") ?? "")));
                if (config.IncludeAddress) 
                    table.AddCell(new Cell().Add(new Paragraph(member.FormattedAddress ?? "")));
                if (config.IncludePhoneNumber) 
                    table.AddCell(new Cell().Add(new Paragraph(member.PhoneNumber ?? "")));
                if (config.IncludeEmailAddress) 
                    table.AddCell(new Cell().Add(new Paragraph(member.EmailAddress ?? "")));
            }

            document.Add(table);
            document.Close();

            return Task.FromResult(stream.ToArray());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            throw;
        }
    }

    public Task<byte[]> GenerateExcelReportAsync(GroupDto group, List<PersonDto> members, ReportModel config)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add($"Rapport {group.Name}");

        var row = 1;

        // Title
        worksheet.Cells[row, 1].Value = $"Groepsrapport: {group.Name}";
        worksheet.Cells[row, 1].Style.Font.Size = 16;
        worksheet.Cells[row, 1].Style.Font.Bold = true;
        row += 2;

        // Timestamp
        worksheet.Cells[row, 1].Value = $"Gegenereerd op: {DateTime.Now:dd-MM-yyyy HH:mm}";
        worksheet.Cells[row, 1].Style.Font.Size = 10;
        worksheet.Cells[row, 1].Style.Font.Color.SetColor(System.Drawing.Color.Gray);
        row++;

        // Coordinator if present
        if (!string.IsNullOrEmpty(group.CoordinatorName))
        {
            worksheet.Cells[row, 1].Value = $"Coördinator: {group.CoordinatorName}";
            worksheet.Cells[row, 1].Style.Font.Size = 12;
            row++;
        }

        row += 2; // Add some space

        // Headers
        var col = 1;
        worksheet.Cells[row, col++].Value = "Volledige naam";
        if (config.IncludeDateOfBirth) worksheet.Cells[row, col++].Value = "Geboortedatum";
        if (config.IncludeAddress) worksheet.Cells[row, col++].Value = "Adres";
        if (config.IncludePhoneNumber) worksheet.Cells[row, col++].Value = "Telefoon";
        if (config.IncludeEmailAddress) worksheet.Cells[row, col++].Value = "E-mail";

        // Style headers
        using (var range = worksheet.Cells[row, 1, row, col - 1])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }

        row++;

        // Data rows
        foreach (var member in members)
        {
            col = 1;
            worksheet.Cells[row, col++].Value = member.FullName ?? "";
            if (config.IncludeDateOfBirth) 
                worksheet.Cells[row, col++].Value = member.DateOfBirth?.ToString("dd-MM-yyyy") ?? "";
            if (config.IncludeAddress) 
                worksheet.Cells[row, col++].Value = member.FormattedAddress ?? "";
            if (config.IncludePhoneNumber) 
                worksheet.Cells[row, col++].Value = member.PhoneNumber ?? "";
            if (config.IncludeEmailAddress) 
                worksheet.Cells[row, col++].Value = member.EmailAddress ?? "";

            // Add border to data rows
            using (var range = worksheet.Cells[row, 1, row, col - 1])
            {
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            row++;
        }

        // Auto-fit columns
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        return Task.FromResult(package.GetAsByteArray());
    }

    private static int GetColumnCount(ReportModel config)
    {
        int count = 1; // Always include full name
        if (config.IncludeDateOfBirth) count++;
        if (config.IncludeAddress) count++;
        if (config.IncludePhoneNumber) count++;
        if (config.IncludeEmailAddress) count++;
        return count;
    }

    private static void AddTableHeader(Table table, string text)
    {
        var cell = new Cell().Add(new Paragraph(text));
        cell.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
        cell.SetBold();
        table.AddHeaderCell(cell);
    }
}
