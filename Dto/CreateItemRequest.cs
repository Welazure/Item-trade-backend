using System.ComponentModel.DataAnnotations;

namespace Trading.Dto;

public class CreateItemRequest
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Request { get; set; }
}
