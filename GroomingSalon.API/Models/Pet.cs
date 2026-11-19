using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GroomingSalon.API.Models;

public partial class Pet
{

    public uint Id { get; set; }

    public uint UserId { get; set; }

    public string Name { get; set; } = null!;

    public byte? Age { get; set; }

    public string? Breed { get; set; }

    public string? Size { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    [ValidateNever]
    public virtual User User { get; set; } = null!;

    public string? ImageUrl { get; set; }
}
