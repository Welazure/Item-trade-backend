using System.ComponentModel.DataAnnotations;

namespace Trading.Dto;

public class CategoryCreate
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

}