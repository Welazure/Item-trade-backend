using System.ComponentModel.DataAnnotations;

namespace Trading.Dto;

public class CategoryDelete
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

}