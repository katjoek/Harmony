using Harmony.ApplicationCore.DTOs;
using LiteBus.Queries.Abstractions;

namespace Harmony.ApplicationCore.Queries.Persons;

public sealed record GetPersonByIdQuery(string Id) : IQuery<PersonDto?>;
