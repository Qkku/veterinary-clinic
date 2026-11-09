using System.ComponentModel.DataAnnotations;

namespace GroomingSalon.API.DTOs;

public class RegisterUserDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = null!;

    [StringLength(30)]
    public string? Phone { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = null!;
}