using MediatR;

namespace Harmony.ApplicationCore.Commands.Membership;

public sealed record RemovePersonFromGroupCommand(
    string PersonId,
    string GroupId) : IRequest;
