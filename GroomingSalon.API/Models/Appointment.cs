using System;
using System.Collections.Generic;

namespace GroomingSalon.API.Models;

public partial class Appointment
{
    public uint Id { get; set; }

    public uint UserId { get; set; }

    public uint PetId { get; set; }

    public uint GroomerId { get; set; }

    public uint ServiceId { get; set; }

    public DateTime StartDatetime { get; set; }

    public DateTime EndDatetime { get; set; }

    public string Status { get; set; } = null!;

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Groomer Groomer { get; set; } = null!;

    public virtual Pet Pet { get; set; } = null!;

    public virtual Review? Review { get; set; }

    public virtual Service Service { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
