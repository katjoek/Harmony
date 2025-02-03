// Harmony.Web/Shared/Components/PersonFormModel.cs
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Harmony.Web.Shared.Components;

public class PersonFormModel
{
    [Required, MaxLength(20)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? MiddleName { get; set; }

    [MaxLength(30)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(80)]
    public string StreetAndHouseNumber { get; set; } = string.Empty;

    [MaxLength(30)]
    public string City { get; set; } = string.Empty;

    [MaxLength(7)]
    public string ZipCode { get; set; } = string.Empty;

    [MaxLength(12)]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(128), EmailAddress]
    public string? EmailAddress { get; set; }

    public DateOnly? DateOfBirth { get; set; }
}