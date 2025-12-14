namespace Harmony.ApplicationCore.DTOs;

public sealed record PersonDto(
    string Id,
    string FirstName,
    string? Prefix,
    string? Surname,
    DateOnly? DateOfBirth,
    string? Street,
    string? ZipCode,
    string? City,
    string? PhoneNumber,
    string? EmailAddress,
    IReadOnlyList<string> GroupIds)
{
    public string FullName => string.Join(" ", new[] { FirstName, Prefix, Surname }.Where(x => !string.IsNullOrEmpty(x)));
    
    public string FormattedAddress
    {
        get
        {
            var parts = new[] { Street, $"{ZipCode} {City}".Trim() }
                .Where(x => !string.IsNullOrWhiteSpace(x));
            return string.Join(", ", parts);
        }
    }
}
