using System.ComponentModel.DataAnnotations;

namespace Harmony.Web.Models;

public sealed class GroupViewModel
{
    public string? Id { get; set; }
    
    [Required(ErrorMessage = "Groepsnaam is verplicht")]
    [Display(Name = "Groepsnaam")]
    public string Name { get; set; } = string.Empty;
    
    [Display(Name = "Coördinator")]
    public string? CoordinatorId { get; set; }
}
