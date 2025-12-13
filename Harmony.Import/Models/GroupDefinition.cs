namespace Harmony.Import.Models;

public sealed record GroupDefinition(
    string Code,
    string Name,
    string? CoordinatorName)
{
    public override string ToString() => $"{Code} - {Name} - {CoordinatorName ?? "<No coordinator>"}";
}