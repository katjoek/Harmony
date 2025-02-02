using Harmony.Core.Entities;

namespace Harmony.Application.Features.People.Queries;

public class PersonDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string MiddleName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string StreetAndHouseNumber { get; set; } = "";
    public string City { get; set; } = "";
    public string ZipCode { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string EmailAddress { get; set; } = "";
    public DateOnly? DateOfBirth { get; set; }

    public string FullName => GetFullName();

    public PersonDto FromEntity(Person person)
    {
        return new PersonDto
        {
            Id = person.Id,
            FirstName = person.FirstName,
            MiddleName = person.MiddleName,
            LastName = person.LastName,
            StreetAndHouseNumber = person.StreetAndHouseNumber,
            City = person.City,
            ZipCode = person.ZipCode,
            PhoneNumber = person.PhoneNumber,
            EmailAddress = person.EmailAddress,
            DateOfBirth = person.DateOfBirth
        };
    }

    private string GetFullName()
    {
        var result = $"{FirstName} {MiddleName}".Trim();
        result = $"{result} {LastName}".Trim();
        return result;
    }
}