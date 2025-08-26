using System.Text.RegularExpressions;

namespace Harmony.Domain.ValueObjects;

public sealed record PhoneNumber
{
    private static readonly Regex PhoneRegex = new(
        @"^[\+]?[\d\s\-\(\)\.]{10,}$",
        RegexOptions.Compiled);

    public string Value { get; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be empty", nameof(value));

        var trimmedValue = value.Trim();
        if (!PhoneRegex.IsMatch(trimmedValue))
            throw new ArgumentException("Invalid phone number format", nameof(value));

        Value = trimmedValue;
    }

    public static PhoneNumber? FromString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return new PhoneNumber(value);
    }

    public override string ToString() => Value;
}
