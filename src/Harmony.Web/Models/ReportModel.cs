using System.ComponentModel.DataAnnotations;

namespace Harmony.Web.Models;

public class ReportModel
{
    public string? SelectedGroupId { get; set; }
    public string ReportType { get; set; } = "Group"; // Group | Birthday
    public int SelectedMonth { get; set; } = DateTime.Now.Month; // 1-12
    
    public bool IncludeFullName { get; set; } = true; // Always true, required
    public bool IncludeDateOfBirth { get; set; } = true;
    public bool IncludeAddress { get; set; } = true;
    public bool IncludePhoneNumber { get; set; } = true;
    public bool IncludeEmailAddress { get; set; } = true;
    
    [Required(ErrorMessage = "Selecteer een uitvoerformaat")]
    public string OutputFormat { get; set; } = "PDF";

    [Required(ErrorMessage = "Selecteer een oriëntatie")]
    public string Orientation { get; set; } = "Portrait"; // Portrait | Landscape

    [Required(ErrorMessage = "Selecteer een sorteervolgorde")]
    public string SortOrder { get; set; } = "LastName"; // FirstName | LastName
}
