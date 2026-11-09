using System;
using System.Collections.Generic;

namespace GroomingSalon.API.Models;

public partial class GroomerSchedule
{
    public uint Id { get; set; }

    public uint GroomerId { get; set; }

    /// <summary>
    /// 1=Пн ... 7=Вс
    /// </summary>
    public sbyte DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public ushort SlotDurationMinutes { get; set; }

    public bool? IsActive { get; set; }

    public virtual Groomer Groomer { get; set; } = null!;
}
