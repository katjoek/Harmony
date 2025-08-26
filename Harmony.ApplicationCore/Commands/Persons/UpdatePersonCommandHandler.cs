using Harmony.ApplicationCore.Interfaces;
using Harmony.Domain.ValueObjects;
using MediatR;

namespace Harmony.ApplicationCore.Commands.Persons;

public sealed class UpdatePersonCommandHandler : IRequestHandler<UpdatePersonCommand>
{
    private readonly IPersonRepository _personRepository;

    public UpdatePersonCommandHandler(IPersonRepository personRepository)
    {
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
    }

    public async Task Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
    {
        var personId = PersonId.From(request.Id);
        var person = await _personRepository.GetByIdAsync(personId, cancellationToken);
        
        if (person == null)
            throw new InvalidOperationException($"Person with ID {request.Id} not found");

        var name = new PersonName(request.FirstName, request.Prefix, request.Surname);
        person.UpdateName(name);

        person.UpdateDateOfBirth(request.DateOfBirth);

        var address = new Address(request.Street, request.ZipCode, request.City);
        person.UpdateAddress(address.IsEmpty ? null : address);

        var phoneNumber = PhoneNumber.FromString(request.PhoneNumber);
        person.UpdatePhoneNumber(phoneNumber);

        var emailAddress = EmailAddress.FromString(request.EmailAddress);
        person.UpdateEmailAddress(emailAddress);

        await _personRepository.UpdateAsync(person, cancellationToken);
    }
}
