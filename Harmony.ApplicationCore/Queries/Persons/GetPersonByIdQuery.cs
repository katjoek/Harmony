using Harmony.ApplicationCore.DTOs;
using MediatR;

namespace Harmony.ApplicationCore.Queries.Persons;

public sealed record GetPersonByIdQuery(string Id) : IRequest<PersonDto?>;
