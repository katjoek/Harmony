using System.ComponentModel.DataAnnotations;

namespace Harmony.Web.Models;

public sealed class PersonViewModel
{
    public string? Id { get; set; }
    
    [Required(ErrorMessage = "Voornaam is verplicht")]
    [Display(Name = "Voornaam")]
    public string FirstName { get; set; } = string.Empty;
    
    [Display(Name = "Tussenvoegsel")]
    public string? Prefix { get; set; }
    
    [Display(Name = "Achternaam")]
    public string? Surname { get; set; }
    
    [Display(Name = "Geboortedatum")]
    public DateOnly? DateOfBirth { get; set; }
    
    [Display(Name = "Straat en huisnummer")]
    public string? Street { get; set; }
    
    [Display(Name = "Postcode")]
    public string? ZipCode { get; set; }
    
    [Display(Name = "Plaats")]
    public string? City { get; set; }
    
    [Display(Name = "Telefoon")]
    public string? PhoneNumber { get; set; }
    
    [Display(Name = "E-mail")]
    [EmailAddress(ErrorMessage = "Ongeldig e-mailadres")]
    public string? EmailAddress { get; set; }
}
