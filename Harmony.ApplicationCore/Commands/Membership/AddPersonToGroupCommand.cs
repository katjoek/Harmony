using MediatR;

namespace Harmony.ApplicationCore.Commands.Membership;

public sealed record AddPersonToGroupCommand(
    string PersonId,
    string GroupId) : IRequest;
