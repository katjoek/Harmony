using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Harmony.Import.Models;

namespace Harmony.Import.Services;

public sealed class CsvParserService : ICsvParserService
{
    public IReadOnlyList<PersonData> ParsePersonsSheet(string filePath, IReadOnlyDictionary<string, string> abbreviationToGroupNameMap)
    {
        var lines = ReadCsvFile(filePath);
        if (lines.Length < 3)
            throw new InvalidOperationException("Het Personenbestand moet minimaal 3 rijen bevatten (header, kolomnamen en ten minste één persoon)");

        // Row 2 (index 1) contains column headers with abbreviations - find group code columns
        var headerRow = lines[1].Split(';');
        
        // Map column index to group name using abbreviations from Groups & Coordinators sheet
        var groupColumnIndexToNameMap = new Dictionary<int, string>();
        
        // Map abbreviations to full group names from Groups & Coordinators sheet
        for (int i = 0; i < headerRow.Length; i++)
        {
            var abbreviation = headerRow[i].Trim();
            
            // Check if this is a group code column (2-letter abbreviation)
            if (abbreviation.Length == 2 && char.IsLetter(abbreviation[0]) && char.IsLetter(abbreviation[1]))
            {
                // Look up the group name from Groups & Coordinators sheet using the abbreviation
                if (abbreviationToGroupNameMap.TryGetValue(abbreviation, out var groupName))
                {
                    groupColumnIndexToNameMap[i] = groupName;
                }
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

            // Skip rows where the first name is "aantal personen" (case-insensitive)
            // This is typically a summary row, not an actual person
            if (firstName.Equals("aantal personen", StringComparison.OrdinalIgnoreCase))
                continue;

            var prefix = string.IsNullOrWhiteSpace(columns[2]) ? null : columns[2].Trim();
            var surname = string.IsNullOrWhiteSpace(columns[3]) ? null : columns[3].Trim();
            
            // Parse date of birth (format: DD-MM-YY)
            DateOnly? dateOfBirth = null;
            var dobString = columns[11].Trim();
            if (!string.IsNullOrWhiteSpace(dobString))
            {
                dateOfBirth = ParseDateOfBirth(dobString);
            }

            var street = string.IsNullOrWhiteSpace(columns[8]) ? null : columns[8].Trim();
            var zipCode = string.IsNullOrWhiteSpace(columns[9]) ? null : columns[9].Trim();
            var city = string.IsNullOrWhiteSpace(columns[10]) ? null : columns[10].Trim();
            var phoneNumber = string.IsNullOrWhiteSpace(columns[6]) ? null : columns[6].Trim();
            var emailAddress = string.IsNullOrWhiteSpace(columns[7]) ? null : columns[7].Trim();

            // Extract group memberships - map abbreviations to full names
            var groupNames = new List<string>();
            
            // Iterate through group columns to find memberships
            foreach (var kvp in groupColumnIndexToNameMap)
            {
                var colIndex = kvp.Key;
                var fullGroupName = kvp.Value;
                
                if (colIndex < columns.Length)
                {
                    var cellValue = columns[colIndex].Trim();
                    // Check if cell contains the group code (case-insensitive, handle whitespace)
                    // The cell might contain just the code, or the code with extra text
                    if (!string.IsNullOrWhiteSpace(cellValue))
                    {
                        // Get the abbreviation from the header row to match against cell value
                        var abbreviation = colIndex < headerRow.Length ? headerRow[colIndex].Trim() : string.Empty;
                        
                        // Check if cell starts with or equals the group code
                        if (!string.IsNullOrWhiteSpace(abbreviation) &&
                            (cellValue.Equals(abbreviation, StringComparison.OrdinalIgnoreCase) ||
                             cellValue.StartsWith(abbreviation, StringComparison.OrdinalIgnoreCase)))
                        {
                            // Add the full group name (not the abbreviation)
                            groupNames.Add(fullGroupName);
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
                groupNames));
        }

        return persons;
    }

    public IReadOnlyList<GroupDefinition> ParseGroupsAndCoordinatorsSheet(string filePath)
    {
        var lines = ReadCsvFile(filePath);
        if (lines.Length < 3)
            throw new InvalidOperationException("Het Groepen & Coördinatorenbestand moet minimaal 3 rijen bevatten");

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
            // Coordinator name is in column 3 (index 3), column 2 is empty
            var coordinatorName = columns.Length > 3 && !string.IsNullOrWhiteSpace(columns[3]) 
                ? columns[3].Trim() 
                : null;

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
                continue;

            groups.Add(new GroupDefinition(code, name, coordinatorName));
        }

        return groups;
    }

    private static string[] ReadCsvFile(string filePath)
    {
        // Excel on Windows with Dutch regional settings saves CSV files as Windows-1252
        // Try Windows-1252 first, then fall back to UTF-8 if needed
        try
        {
            // Windows-1252 encoding (Western European, includes Dutch characters like é, ë, etc.)
            var windows1252 = Encoding.GetEncoding(1252);
            return File.ReadAllLines(filePath, windows1252);
        }
        catch (ArgumentException)
        {
            // Code page 1252 not available (shouldn't happen after registering provider, but handle gracefully)
            // Fall back to UTF-8
            return File.ReadAllLines(filePath, Encoding.UTF8);
        }
        catch
        {
            // Any other error reading with Windows-1252, try UTF-8
            try
            {
                return File.ReadAllLines(filePath, Encoding.UTF8);
            }
            catch
            {
                // Last resort: use system default encoding
                return File.ReadAllLines(filePath, Encoding.Default);
            }
        }
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
