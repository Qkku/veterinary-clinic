using System;
using System.Collections.Generic;

namespace GroomingSalon.API.Models;

public partial class Groomer
{
    public uint Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = null!;

    public byte? Age { get; set; }

    public string? Education { get; set; }

    public ushort? ExperienceYears { get; set; }

    public string? Specialization { get; set; }

    public bool? IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<GroomerSchedule> GroomerSchedules { get; set; } = new List<GroomerSchedule>();

    public virtual ICollection<GroomerTimeOff> GroomerTimeOffs { get; set; } = new List<GroomerTimeOff>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
