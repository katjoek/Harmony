using MediatR;

namespace Harmony.Application.Features.People.Commands.DeletePerson;

public sealed record DeletePersonCommand(int Id) : IRequest;