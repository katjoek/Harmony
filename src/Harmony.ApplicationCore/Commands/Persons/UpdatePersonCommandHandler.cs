using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.ValueObjects;
using LiteBus.Commands.Abstractions;

namespace Harmony.ApplicationCore.Commands.Persons;

public sealed class UpdatePersonCommandHandler : ICommandHandler<UpdatePersonCommand>
{
    private readonly IPersonRepository _personRepository;

    public UpdatePersonCommandHandler(IPersonRepository personRepository)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
    }

    public async Task HandleAsync(UpdatePersonCommand command, CancellationToken cancellationToken)
    {
        var personId = PersonId.From(command.Id);
        var person = await _personRepository.GetByIdAsync(personId, cancellationToken);
        
        if (person == null)
            throw new InvalidOperationException($"Person with ID {command.Id} not found");

        var name = new PersonName(command.FirstName, command.Prefix, command.Surname);
        person.UpdateName(name);

        person.UpdateDateOfBirth(command.DateOfBirth);

        var address = new Address(command.Street, command.ZipCode, command.City);
        person.UpdateAddress(address.IsEmpty ? null : address);

        var phoneNumber = PhoneNumber.FromString(command.PhoneNumber);
        person.UpdatePhoneNumber(phoneNumber);

        var emailAddress = EmailAddress.FromString(command.EmailAddress);
        person.UpdateEmailAddress(emailAddress);

        await _personRepository.UpdateAsync(person, cancellationToken);
    }
}
