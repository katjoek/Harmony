using System.Globalization;
using System.IO;
using Harmony.Import.Models;

namespace Harmony.Import.Services;

public sealed class CsvParserService : ICsvParserService
{
    public IReadOnlyList<PersonData> ParseSheet1(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        if (lines.Length < 3)
            throw new InvalidOperationException("Sheet 1 must have at least 3 rows (header, column names, and at least one person)");

        // Row 2 contains column headers - find group code columns
        var headerRow = lines[1].Split(';');
        var groupCodeColumns = new Dictionary<int, string>();
        
        // Find columns that contain group codes (AA, AR, AV, etc.)
        for (int i = 0; i < headerRow.Length; i++)
        {
            var header = headerRow[i].Trim();
            if (header.Length == 2 && char.IsLetter(header[0]) && char.IsLetter(header[1]))
            {
                groupCodeColumns[i] = header.ToUpperInvariant();
            }
        }

        var persons = new List<PersonData>();

        // Process person rows (starting from row 3, index 2)
        for (int rowIndex = 2; rowIndex < lines.Length; rowIndex++)
        {
            var line = lines[rowIndex];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var columns = line.Split(';');
            if (columns.Length < 12)
                continue;

            // Extract person data
            var firstName = columns[1].Trim();
            if (string.IsNullOrWhiteSpace(firstName))
                continue; // Skip rows without first name

            var prefix = string.IsNullOrWhiteSpace(columns[2]) ? null : columns[2].Trim();
            var surname = string.IsNullOrWhiteSpace(columns[3]) ? null : columns[3].Trim();
            
            // Parse date of birth (format: DD-MM-YY)
            DateOnly? dateOfBirth = null;
            var dobString = columns[11].Trim();
            if (!string.IsNullOrWhiteSpace(dobString))
            {
                dateOfBirth = ParseDateOfBirth(dobString);
            }

            var street = string.IsNullOrWhiteSpace(columns[7]) ? null : columns[7].Trim();
            var zipCode = string.IsNullOrWhiteSpace(columns[8]) ? null : columns[8].Trim();
            var city = string.IsNullOrWhiteSpace(columns[9]) ? null : columns[9].Trim();
            var phoneNumber = string.IsNullOrWhiteSpace(columns[5]) ? null : columns[5].Trim();
            var emailAddress = string.IsNullOrWhiteSpace(columns[6]) ? null : columns[6].Trim();

            // Extract group memberships
            var groupCodes = new List<string>();
            foreach (var kvp in groupCodeColumns)
            {
                if (kvp.Key < columns.Length)
                {
                    var cellValue = columns[kvp.Key].Trim();
                    // Check if cell contains the group code (case-insensitive, handle whitespace)
                    // The cell might contain just the code, or the code with extra text
                    if (!string.IsNullOrWhiteSpace(cellValue))
                    {
                        // Check if cell starts with or equals the group code
                        if (cellValue.Equals(kvp.Value, StringComparison.OrdinalIgnoreCase) ||
                            cellValue.StartsWith(kvp.Value, StringComparison.OrdinalIgnoreCase))
                        {
                            groupCodes.Add(kvp.Value);
                        }
                    }
                }
            }

            persons.Add(new PersonData(
                firstName,
                prefix,
                surname,
                dateOfBirth,
                street,
                zipCode,
                city,
                phoneNumber,
                emailAddress,
                groupCodes));
        }

        return persons;
    }

    public IReadOnlyList<GroupDefinition> ParseSheet2(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        if (lines.Length < 3)
            throw new InvalidOperationException("Sheet 2 must have at least 3 rows");

        var groups = new List<GroupDefinition>();

        // Skip header row (index 0) and instruction row (index 1)
        // Process until we hit the footer instruction row
        for (int i = 2; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Check for footer instruction row
            if (line.Contains("Nieuwe groepen toevoegen boven deze rij", StringComparison.OrdinalIgnoreCase))
                break;

            var columns = line.Split(';');
            if (columns.Length < 3)
                continue;

            var code = columns[0].Trim();
            var name = columns[1].Trim();
            var coordinatorName = columns.Length > 2 && !string.IsNullOrWhiteSpace(columns[2]) 
                ? columns[2].Trim() 
                : null;

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                continue;

            groups.Add(new GroupDefinition(code, name, coordinatorName));
        }

        return groups;
    }

    private static DateOnly? ParseDateOfBirth(string dateString)
    {
        // Expected format: DD-MM-YY (e.g., "05-08-37")
        if (string.IsNullOrWhiteSpace(dateString))
            return null;

        var parts = dateString.Split('-');
        if (parts.Length != 3)
            return null;

        if (!int.TryParse(parts[0], out var day) ||
            !int.TryParse(parts[1], out var month) ||
            !int.TryParse(parts[2], out var year))
            return null;

        // Convert 2-digit year to 4-digit year
        // If year < 50, assume 2000s, else assume 1900s
        var fullYear = year < 50 ? 2000 + year : 1900 + year;

        try
        {
            return new DateOnly(fullYear, month, day);
        }
        catch
        {
            return null;
        }
    }
}
