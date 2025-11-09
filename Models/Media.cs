using System.ComponentModel.DataAnnotations;

namespace Trading.Models;

public class Media
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid ItemId { get; set; }

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; }

    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; }

    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; }

    [Required]
    public long FileSize { get; set; }

    [Required]
    [MaxLength(20)]
    public string MediaType { get; set; } // "Image" or "Video"

    public bool IsPrimary { get; set; } = false;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public virtual Item Item { get; set; }
}
