using MediatR;

namespace Harmony.Application.Features.People.Queries.GetPerson;

public sealed record GetPersonQuery(int Id) : IRequest<PersonDto>;