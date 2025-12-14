using LiteBus.Commands.Abstractions;

namespace Harmony.ApplicationCore.Commands.Membership;

public sealed record AddPersonToGroupCommand(
    string PersonId,
    string GroupId) : ICommand;
