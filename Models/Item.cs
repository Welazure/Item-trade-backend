using System.ComponentModel.DataAnnotations;


namespace Trading.Models;

public class Item
{
    [Required]
    public Guid Id { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
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

    [Required]
    public bool IsApproved { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Category Category { get; set; }
    public virtual User User { get; set; }
    public virtual ICollection<Media> Media { get; set; }
    public virtual ICollection<Booking> Bookings { get; set; }
}