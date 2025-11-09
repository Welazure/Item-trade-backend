using System.ComponentModel.DataAnnotations;
using Trading.Models;

namespace Trading.Dto;

public class UserRegister
{
    [Required]
    [MaxLength(20)]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
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
}