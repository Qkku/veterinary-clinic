
using System.ComponentModel.DataAnnotations;

namespace GroomingSalon.API.DTOs;

public class UpdateGroomerDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    public byte? Age { get; set; }

    [StringLength(255)]
    public string? Education { get; set; }

    public ushort? ExperienceYears { get; set; }

    [StringLength(150)]
    public string? Specialization { get; set; }

    public bool IsActive { get; set; }
}
