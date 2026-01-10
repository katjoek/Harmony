namespace Harmony.Domain.Entities;

public sealed class Config
{
    public string Key { get; private set; }
    public string? Value { get; private set; }

    // Required for EF Core
    private Config()
    {
        Key = null!;
    }

    public Config(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        Key = key;
        Value = value;
    }

    public void UpdateValue(string? value)
    {
        Value = value;
    }
}
