using System.ComponentModel.DataAnnotations;

namespace Harmony.Core.Entities;

public class Group : BaseEntity
{
    [MaxLength(30)]
    public string Name { get; set; } = null!;
    
    public int CoordinatorId { get; set; }
    
    public Person Coordinator { get; set; } = null!;
 
    public List<Person> Members { get; set; } = new();
}