using MediatR;

namespace Harmony.ApplicationCore.Commands.Groups;

public sealed record DeleteGroupCommand(string Id) : IRequest;
