using MediatR;

namespace Harmony.Application.Features.People.Commands.UpdatePerson;

public sealed record UpdatePersonCommand(
    int Id,
    string FirstName,
    string? MiddleName,
    string LastName,
    string StreetAndHouseNumber,
    string City,
    string ZipCode,
    string PhoneNumber,
    string EmailAddress,
    DateOnly? DateOfBirth) : IRequest;