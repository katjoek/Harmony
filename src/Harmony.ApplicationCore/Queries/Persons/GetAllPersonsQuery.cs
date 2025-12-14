using Harmony.ApplicationCore.DTOs;
using LiteBus.Queries.Abstractions;

namespace Harmony.ApplicationCore.Queries.Persons;

public sealed record GetAllPersonsQuery : IQuery<IReadOnlyList<PersonDto>>;
