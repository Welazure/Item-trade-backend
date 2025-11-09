using System.ComponentModel.DataAnnotations;

namespace Trading.Dto;

public class UpdateItemRequest
{
    [MaxLength(50)]
    public string? Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }

    [MaxLength(500)]
    public string? Request { get; set; }
}
