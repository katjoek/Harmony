using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.Entities;
using Harmony.Domain.ValueObjects;
using MediatR;

namespace Harmony.ApplicationCore.Commands.Persons;

public sealed class CreatePersonCommandHandler : IRequestHandler<CreatePersonCommand, string>
{
    private readonly IPersonRepository _personRepository;

    public CreatePersonCommandHandler(IPersonRepository personRepository)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
    }

    public async Task<string> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
    {
        var name = new PersonName(request.FirstName, request.Prefix, request.Surname);
        var person = Person.Create(name);

        if (request.DateOfBirth.HasValue)
            person.UpdateDateOfBirth(request.DateOfBirth.Value);

        if (!string.IsNullOrWhiteSpace(request.Street) || 
            !string.IsNullOrWhiteSpace(request.ZipCode) || 
            !string.IsNullOrWhiteSpace(request.City))
        {
            var address = new Address(request.Street, request.ZipCode, request.City);
            person.UpdateAddress(address);
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var phoneNumber = PhoneNumber.FromString(request.PhoneNumber);
            person.UpdatePhoneNumber(phoneNumber);
        }

        if (!string.IsNullOrWhiteSpace(request.EmailAddress))
        {
            var emailAddress = EmailAddress.FromString(request.EmailAddress);
            person.UpdateEmailAddress(emailAddress);
        }

        await _personRepository.AddAsync(person, cancellationToken);

        return person.Id.ToString();
    }
}
