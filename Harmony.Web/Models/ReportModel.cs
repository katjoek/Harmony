using System.ComponentModel.DataAnnotations;

namespace Harmony.Web.Models;

public class ReportModel
{
    [Required(ErrorMessage = "Selecteer een groep")]
    public string? SelectedGroupId { get; set; }
    
    public bool IncludeFullName { get; set; } = true; // Always true, required
    public bool IncludeDateOfBirth { get; set; } = true;
    public bool IncludeAddress { get; set; } = true;
    public bool IncludePhoneNumber { get; set; } = true;
    public bool IncludeEmailAddress { get; set; } = true;
    
    [Required(ErrorMessage = "Selecteer een uitvoerformaat")]
    public string OutputFormat { get; set; } = "PDF";
}
