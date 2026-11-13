using System.ComponentModel.DataAnnotations;

namespace GroomingSalon.API.DTOs;

public class LoginUserDto
{
    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}
