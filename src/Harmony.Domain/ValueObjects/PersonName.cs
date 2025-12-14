namespace Harmony.Domain.ValueObjects;

public sealed record PersonName
{
    public string FirstName { get; }
    public string? Prefix { get; }
    public string? Surname { get; }

    public PersonName(string firstName, string? prefix = null, string? surname = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));
        
        FirstName = firstName.Trim();
        Prefix = string.IsNullOrWhiteSpace(prefix) ? null : prefix.Trim();
        Surname = string.IsNullOrWhiteSpace(surname) ? null : surname.Trim();
    }

    public string FullName => string.Join(" ", new[] { FirstName, Prefix, Surname }.Where(x => !string.IsNullOrEmpty(x)));

    public override string ToString() => FullName;
}
