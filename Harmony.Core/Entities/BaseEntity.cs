using System.ComponentModel.DataAnnotations;

namespace Harmony.Core.Entities;

public class BaseEntity
{
    [Key]
    public int Id { get; set; }
}