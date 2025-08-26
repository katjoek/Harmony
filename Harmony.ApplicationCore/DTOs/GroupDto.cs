namespace Harmony.ApplicationCore.DTOs;

public sealed record GroupDto(
    string Id,
    string Name,
    string? CoordinatorId,
    string? CoordinatorName,
    IReadOnlyList<string> MemberIds,
    int MemberCount);
