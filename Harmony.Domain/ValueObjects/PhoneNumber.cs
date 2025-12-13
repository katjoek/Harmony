using System.Text.RegularExpressions;

namespace Harmony.Domain.ValueObjects;

public sealed record PhoneNumber
{

    public string Value { get; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be empty", nameof(value));

        Value = value.Trim();
    }

    public static PhoneNumber? FromString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return new PhoneNumber(value);
    }

    public override string ToString() => Value;
}
