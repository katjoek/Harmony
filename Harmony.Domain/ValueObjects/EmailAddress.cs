using System.Text.RegularExpressions;

namespace Harmony.Domain.ValueObjects;

public sealed record EmailAddress
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email address cannot be empty", nameof(value));

        var trimmedValue = value.Trim();
        if (!EmailRegex.IsMatch(trimmedValue))
            throw new ArgumentException("Invalid email address format", nameof(value));

        Value = trimmedValue.ToLowerInvariant();
    }

    public static EmailAddress? FromString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return new EmailAddress(value);
    }

    public override string ToString() => Value;
}
