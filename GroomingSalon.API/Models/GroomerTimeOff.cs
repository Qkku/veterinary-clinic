using System;
using System.Collections.Generic;

namespace GroomingSalon.API.Models;

public partial class GroomerTimeOff
{
    public uint Id { get; set; }

    public uint GroomerId { get; set; }

    public DateTime StartDatetime { get; set; }

    public DateTime EndDatetime { get; set; }

    public string Type { get; set; } = null!;

    public string? Comment { get; set; }

    public virtual Groomer Groomer { get; set; } = null!;
}
