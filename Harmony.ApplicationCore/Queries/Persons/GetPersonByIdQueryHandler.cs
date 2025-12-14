using Harmony.ApplicationCore.DTOs;
using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.ValueObjects;
using LiteBus.Queries.Abstractions;

namespace Harmony.ApplicationCore.Queries.Persons;

public sealed class GetPersonByIdQueryHandler : IQueryHandler<GetPersonByIdQuery, PersonDto?>
{
    private readonly IPersonRepository _personRepository;

    public GetPersonByIdQueryHandler(IPersonRepository personRepository)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
    }

    public async Task<PersonDto?> HandleAsync(GetPersonByIdQuery query, CancellationToken cancellationToken)
    {
        var personId = PersonId.From(query.Id);
        var person = await _personRepository.GetByIdAsync(personId, cancellationToken);

        if (person == null)
            return null;

        return new PersonDto(
            person.Id.ToString(),
            person.Name.FirstName,
            person.Name.Prefix,
            person.Name.Surname,
            person.DateOfBirth,
            person.Address?.Street,
            person.Address?.ZipCode,
            person.Address?.City,
            person.PhoneNumber?.Value,
            person.EmailAddress?.Value,
            person.GroupIds.Select(g => g.ToString()).ToList());
    }
}
