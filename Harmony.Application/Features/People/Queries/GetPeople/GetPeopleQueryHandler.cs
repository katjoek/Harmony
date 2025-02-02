// Harmony.Application/Features/People/Queries/GetPeople/GetPeopleQueryHandler.cs

using Harmony.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Application.Features.People.Queries.GetPeople;

internal sealed class GetPeopleQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPeopleQuery, List<PersonDto>>
{
    public async Task<List<PersonDto>> Handle(GetPeopleQuery request, CancellationToken cancellationToken)
    {
        return await context.People
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
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
            .ToListAsync(cancellationToken);
    }
}