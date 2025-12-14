using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.Entities;
using Harmony.Domain.ValueObjects;
using LiteBus.Commands.Abstractions;

namespace Harmony.ApplicationCore.Commands.Persons;

public sealed class CreatePersonCommandHandler : ICommandHandler<CreatePersonCommand, string>
{
    private readonly IPersonRepository _personRepository;

    public CreatePersonCommandHandler(IPersonRepository personRepository)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
    }

    public async Task<string> HandleAsync(CreatePersonCommand command, CancellationToken cancellationToken)
    {
        var name = new PersonName(command.FirstName, command.Prefix, command.Surname);
        var person = Person.Create(name);

        if (command.DateOfBirth.HasValue)
            person.UpdateDateOfBirth(command.DateOfBirth.Value);

        if (!string.IsNullOrWhiteSpace(command.Street) || 
            !string.IsNullOrWhiteSpace(command.ZipCode) || 
            !string.IsNullOrWhiteSpace(command.City))
        {
            var address = new Address(command.Street, command.ZipCode, command.City);
            person.UpdateAddress(address);
        }

        if (!string.IsNullOrWhiteSpace(command.PhoneNumber))
        {
            var phoneNumber = PhoneNumber.FromString(command.PhoneNumber);
            person.UpdatePhoneNumber(phoneNumber);
        }

        if (!string.IsNullOrWhiteSpace(command.EmailAddress))
        {
            var emailAddress = EmailAddress.FromString(command.EmailAddress);
            person.UpdateEmailAddress(emailAddress);
        }

        await _personRepository.AddAsync(person, cancellationToken);

        return person.Id.ToString();
    }
}
