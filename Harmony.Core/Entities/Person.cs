using System.ComponentModel.DataAnnotations;

namespace Harmony.Core.Entities;

public class Person : BaseEntity
{
    [MaxLength(20)]
    public string FirstName { get; set; } = "";

    [MaxLength(10)]
    public string MiddleName { get; set; } = "";
    
    [MaxLength(30)]
    public string LastName { get; set; } = "";
    
    [MaxLength(80)]
    public string StreetAndHouseNumber { get; set; } = "";
    
    [MaxLength(30)]
    public string City { get; set; } = "";
    
    [MaxLength(7)]
    public string ZipCode { get; set; } = "";
    
    [MaxLength(12)]
    public string PhoneNumber { get; set; } = "";

    [MaxLength(128), EmailAddress]
    public string? EmailAddress { get; set; } = "";
    
    public DateOnly? DateOfBirth { get; set; }

    public List<Group> Memberships { get; set; } = new();
}