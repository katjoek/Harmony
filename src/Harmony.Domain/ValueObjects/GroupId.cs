namespace Harmony.Domain.ValueObjects;

public sealed record GroupId(Guid Value)
{
    public static GroupId New() => new(Guid.NewGuid());
    
    public static GroupId From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException("Invalid GroupId format", nameof(value));
        
        return new GroupId(guid);
    }
    
    public override string ToString() => Value.ToString();
}
