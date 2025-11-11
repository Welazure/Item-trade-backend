﻿using System.ComponentModel.DataAnnotations;

namespace Trading.Models;

public class User
{
    public Guid Id { get; set; }
    [Required]
    [MaxLength(20)]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    public Role Role { get; set; }
    [Required]
    [EmailAddress]
    [MaxLength(50)]
    public string Email { get; set; }
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    [Required]
    [MaxLength(50)]
    public string Address { get; set; }
    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; }

    public int Points { get; set; } = 2;
    
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Item> Items { get; set; }
    public virtual ICollection<Booking> Bookings { get; set; }
}

public enum Role
{
    Admin,
    User
}