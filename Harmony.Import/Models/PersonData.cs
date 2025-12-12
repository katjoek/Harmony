namespace Harmony.Import.Models;

public sealed record PersonData(
    string FirstName,
    string? Prefix,
    string? Surname,
    DateOnly? DateOfBirth,
    string? Street,
    string? ZipCode,
    string? City,
    string? PhoneNumber,
    string? EmailAddress,
    IReadOnlyList<string> GroupCodes);
