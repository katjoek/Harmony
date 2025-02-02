// Harmony.Application/Features/People/Commands/CreatePerson/CreatePersonCommandHandler.cs
using Harmony.Application.Interfaces;
using Harmony.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Application.Features.People.Commands.CreatePerson;

internal sealed class CreatePersonCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreatePersonCommand, int>
{
    public async Task<int> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
    {
        var person = new Person
        {
            FirstName = request.FirstName,
            MiddleName = request.MiddleName ?? "",
            LastName = request.LastName,
            StreetAndHouseNumber = request.StreetAndHouseNumber,
            City = request.City,
            ZipCode = request.ZipCode,
            PhoneNumber = request.PhoneNumber,
            EmailAddress = request.EmailAddress,
            DateOfBirth = request.DateOfBirth
        };

        if (request.GroupIds != null && request.GroupIds.Any())
        {
            var groups = await context.Groups
                .Where(g => request.GroupIds.Contains(g.Id))
                .ToListAsync(cancellationToken);

            person.Memberships.AddRange(groups);
        }

        context.People.Add(person);
        await context.SaveChangesAsync(cancellationToken);

        return person.Id;
    }
}