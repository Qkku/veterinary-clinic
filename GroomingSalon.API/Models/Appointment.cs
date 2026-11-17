using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Review? Review { get; set; }

    [ValidateNever]
    public string Status { get; set; } = null!;

    [ValidateNever]
    public virtual Groomer Groomer { get; set; } = null!;

    [ValidateNever]
    public virtual Pet Pet { get; set; } = null!;

    [ValidateNever]
    public virtual Service Service { get; set; } = null!;

    [ValidateNever]
    public virtual User User { get; set; } = null!;
}
