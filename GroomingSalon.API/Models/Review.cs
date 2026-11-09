using System;
using System.Collections.Generic;

namespace GroomingSalon.API.Models;

public partial class Review
{
    public uint Id { get; set; }

    public uint UserId { get; set; }

    public uint GroomerId { get; set; }

    public uint? AppointmentId { get; set; }

    public byte Rating { get; set; }

    public string? Text { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Appointment? Appointment { get; set; }

    public virtual Groomer Groomer { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
