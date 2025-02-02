// Harmony.Web/Shared/Components/PersonFormModel.cs
using System.ComponentModel.DataAnnotations;

namespace Harmony.Web.Shared.Components;

public class PersonFormModel
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
}