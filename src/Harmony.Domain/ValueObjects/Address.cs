namespace Harmony.Domain.ValueObjects;

public sealed record Address
{
    public string? Street { get; }
    public string? ZipCode { get; }
    public string? City { get; }

    public Address(string? street = null, string? zipCode = null, string? city = null)
    {
        Street = string.IsNullOrWhiteSpace(street) ? null : street.Trim();
        ZipCode = string.IsNullOrWhiteSpace(zipCode) ? null : zipCode.Trim();
        City = string.IsNullOrWhiteSpace(city) ? null : city.Trim();
    }

    public string FormattedAddress
    {
        get
        {
            var parts = new[] { Street, $"{ZipCode} {City}".Trim() }
                .Where(x => !string.IsNullOrWhiteSpace(x));
            return string.Join(", ", parts);
        }
    }

    public bool IsEmpty => string.IsNullOrWhiteSpace(Street) && 
                          string.IsNullOrWhiteSpace(ZipCode) && 
                          string.IsNullOrWhiteSpace(City);

    public override string ToString() => FormattedAddress;
}
