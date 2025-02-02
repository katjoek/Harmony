using MediatR;

namespace Harmony.Application.Features.People.Queries.GetPeople;

public sealed record GetPeopleQuery : IRequest<List<PersonDto>>;