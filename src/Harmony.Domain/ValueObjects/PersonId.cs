namespace Harmony.Domain.ValueObjects;

public sealed record PersonId(Guid Value)
{
    public static PersonId New() => new(Guid.NewGuid());
    
    public static PersonId From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException("Invalid PersonId format", nameof(value));
        
        return new PersonId(guid);
    }
    
    public override string ToString() => Value.ToString();
}
