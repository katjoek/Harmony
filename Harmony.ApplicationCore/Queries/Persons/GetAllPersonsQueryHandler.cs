using Harmony.ApplicationCore.DTOs;
using Harmony.ApplicationCore.Interfaces;
using LiteBus.Queries.Abstractions;

namespace Harmony.ApplicationCore.Queries.Persons;

public sealed class GetAllPersonsQueryHandler : IQueryHandler<GetAllPersonsQuery, IReadOnlyList<PersonDto>>
{
    private readonly IPersonRepository _personRepository;
    private readonly IMembershipService _membershipService;

    public GetAllPersonsQueryHandler(IPersonRepository personRepository, IMembershipService membershipService)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _membershipService = membershipService ?? throw new ArgumentNullException(nameof(membershipService));
    }

    public async Task<IReadOnlyList<PersonDto>> HandleAsync(GetAllPersonsQuery query, CancellationToken cancellationToken)
    {
        // Get all persons and their memberships in a single optimized query
        var persons = await _personRepository.GetAllAsync(cancellationToken);
        
        // Get all memberships in one query instead of N queries
        var allMemberships = await _membershipService.GetAllMembershipsAsync(cancellationToken);
        
        // Group memberships by person ID for fast lookup
        var membershipsByPerson = allMemberships
            .GroupBy(m => m.PersonId)
            .ToDictionary(g => g.Key, g => g.Select(m => m.GroupId.ToString()).ToList());

        // Build DTOs efficiently
        var result = persons.Select(person => new PersonDto(
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
            membershipsByPerson.GetValueOrDefault(person.Id, new List<string>()))).ToList();

        return result;
    }
}
