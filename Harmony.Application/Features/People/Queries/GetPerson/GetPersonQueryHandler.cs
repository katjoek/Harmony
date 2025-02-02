using Harmony.Application.Interfaces;
using Harmony.Core.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Application.Features.People.Queries.GetPerson;

internal sealed class GetPersonQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPersonQuery, PersonDto>
{
    public async Task<PersonDto> Handle(GetPersonQuery request, CancellationToken cancellationToken)
    {
         var person = await context.People
            .Select(p => new PersonDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                MiddleName = p.MiddleName,
                LastName = p.LastName,
                StreetAndHouseNumber = p.StreetAndHouseNumber,
                City = p.City,
                ZipCode = p.ZipCode,
                PhoneNumber = p.PhoneNumber,
                EmailAddress = p.EmailAddress,
                DateOfBirth = p.DateOfBirth
            })
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

         if (person is null)
         {
             throw new NotFoundException($"Person with ID {request.Id} not found.");
         }
         
         return person;
    }
}