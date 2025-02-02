using System.ComponentModel.DataAnnotations;
using Harmony.Core.Entities;

namespace Harmony.Application.Features.People.Commands;

public sealed class CreatePersonDto
{
    [Required, MaxLength(20)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? MiddleName { get; set; }

    [Required, MaxLength(30)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string StreetAndHouseNumber { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string City { get; set; } = string.Empty;

    [Required, MaxLength(7)]
    public string ZipCode { get; set; } = string.Empty;

    [Required, MaxLength(12)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, MaxLength(128)]
    public string EmailAddress { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }
    
    public CreatePersonDto FromEntity(Person person)
    {
        return new CreatePersonDto
        {
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