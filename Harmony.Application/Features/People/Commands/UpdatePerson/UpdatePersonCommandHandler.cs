using Harmony.Application.Interfaces;
using Harmony.Core.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Application.Features.People.Commands.UpdatePerson;

internal sealed class UpdatePersonCommandHandler : IRequestHandler<UpdatePersonCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdatePersonCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
    {
        var person = await _context.People
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (person == null)
        {
            throw new NotFoundException($"Person with ID {request.Id} not found.");
        }

        person.FirstName = request.FirstName;
        person.MiddleName = request.MiddleName ?? "";
        person.LastName = request.LastName;
        person.StreetAndHouseNumber = request.StreetAndHouseNumber;
        person.City = request.City;
        person.ZipCode = request.ZipCode;
        person.PhoneNumber = request.PhoneNumber;
        person.EmailAddress = request.EmailAddress;
        person.DateOfBirth = request.DateOfBirth;

        await _context.SaveChangesAsync(cancellationToken);
    }
}