using System.ComponentModel.DataAnnotations;

namespace Trading.Models;

public class Booking
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid ItemId { get; set; }

    [Required]
    public Guid BookerUserId { get; set; }

    [Required]
    public DateTime BookedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CancelledAt { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual Item Item { get; set; }
    public virtual User BookerUser { get; set; }
}
