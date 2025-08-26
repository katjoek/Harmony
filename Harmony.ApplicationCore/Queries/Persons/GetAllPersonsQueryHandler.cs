using Harmony.ApplicationCore.DTOs;
using Harmony.ApplicationCore.Interfaces;
using MediatR;

namespace Harmony.ApplicationCore.Queries.Persons;

public sealed class GetAllPersonsQueryHandler : IRequestHandler<GetAllPersonsQuery, IReadOnlyList<PersonDto>>
{
    private readonly IPersonRepository _personRepository;
    private readonly IMembershipService _membershipService;

    public GetAllPersonsQueryHandler(IPersonRepository personRepository, IMembershipService membershipService)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _membershipService = membershipService ?? throw new ArgumentNullException(nameof(membershipService));
    }

    public async Task<IReadOnlyList<PersonDto>> Handle(GetAllPersonsQuery request, CancellationToken cancellationToken)
    {
        var persons = await _personRepository.GetAllAsync(cancellationToken);
        var result = new List<PersonDto>();

        foreach (var person in persons)
        {
            var groupIds = await _membershipService.GetGroupIdsForPersonAsync(person.Id, cancellationToken);
            
            result.Add(new PersonDto(
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
                groupIds.Select(g => g.ToString()).ToList()));
        }

        return result;
    }
}
