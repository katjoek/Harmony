using MediatR;

namespace Harmony.Application.Features.People.Commands.CreatePerson;

public sealed record CreatePersonCommand(
    string FirstName,
    string? MiddleName,
    string LastName,
    string StreetAndHouseNumber,
    string City,
    string ZipCode,
    string PhoneNumber,
    string EmailAddress,
    DateOnly? DateOfBirth,
    List<int>? GroupIds = null) : IRequest<int>;