using LiteBus.Commands.Abstractions;

namespace Harmony.ApplicationCore.Commands.Groups;

public sealed record UpdateGroupCommand(
    string Id,
    string Name,
    string? CoordinatorId) : ICommand;
