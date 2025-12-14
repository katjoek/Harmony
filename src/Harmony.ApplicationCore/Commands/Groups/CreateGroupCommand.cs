using LiteBus.Commands.Abstractions;

namespace Harmony.ApplicationCore.Commands.Groups;

public sealed record CreateGroupCommand(
    string Name,
    string? CoordinatorId) : ICommand<string>;
