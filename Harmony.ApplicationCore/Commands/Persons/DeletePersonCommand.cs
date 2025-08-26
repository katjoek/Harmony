using MediatR;

namespace Harmony.ApplicationCore.Commands.Persons;

public sealed record DeletePersonCommand(string Id) : IRequest;
