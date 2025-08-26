using MediatR;

namespace Harmony.ApplicationCore.Commands.Groups;

public sealed record CreateGroupCommand(
    string Name,
    string? CoordinatorId) : IRequest<string>;
