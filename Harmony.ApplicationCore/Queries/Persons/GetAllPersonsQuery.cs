using Harmony.ApplicationCore.DTOs;
using MediatR;

namespace Harmony.ApplicationCore.Queries.Persons;

public sealed record GetAllPersonsQuery : IRequest<IReadOnlyList<PersonDto>>;
