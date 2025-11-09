using System.ComponentModel.DataAnnotations;

namespace Trading.Models;

public class Category
{
    public Guid Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    
    public virtual ICollection<Item> Items { get; set; }
}