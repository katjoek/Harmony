using MediatR;

namespace Harmony.ApplicationCore.Commands.Groups;

public sealed record UpdateGroupCommand(
    string Id,
    string Name,
    string? CoordinatorId) : IRequest;
