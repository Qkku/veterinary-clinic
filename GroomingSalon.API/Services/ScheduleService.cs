
using GroomingSalon.API.Data;
using Microsoft.EntityFrameworkCore;

namespace GroomingSalon.API.Services;

public class ScheduleService    
{
    private readonly ApplicationDbContext _context;

    public ScheduleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AvailableSlotDto>> GetAvailableSlots(
        DateTime date,
        uint serviceId,
        uint? groomerId = null)
    {
        date = date.Date;
        DateTime nextDay = date.AddDays(1);


        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.IsActive == true);

        if (service == null)
            throw new ArgumentException("Услуга не найдена или неактивна.");

        int duration = service.DurationMinutes;


        int dayOfWeek = ((int)date.DayOfWeek + 6) % 7 + 1;

        var groomersQuery = _context.Groomers
            .Where(g => g.IsActive == true);

        if (groomerId.HasValue)
            groomersQuery = groomersQuery.Where(g => g.Id == groomerId.Value);

        var groomers = await groomersQuery.ToListAsync();


        var schedules = await _context.GroomerSchedules
            .Where(s =>
                s.DayOfWeek == dayOfWeek &&
                s.IsActive == true &&
                s.ValidFrom <= date &&
                (s.ValidTo == null || s.ValidTo >= date) &&
                groomers.Select(g => g.Id).Contains(s.GroomerId))
            .ToListAsync();


        var timeOffs = await _context.GroomerTimeOffs
            .Where(t =>
                t.StartDatetime < nextDay &&
                t.EndDatetime > date &&
                groomers.Select(g => g.Id).Contains(t.GroomerId))
            .ToListAsync();


        var appointments = await _context.Appointments
            .Where(a =>
                a.StartDatetime < nextDay &&
                a.EndDatetime > date &&
                (a.Status == "booked" || a.Status == "confirmed") &&
                groomers.Select(g => g.Id).Contains(a.GroomerId))
            .ToListAsync();

        var result = new Dictionary<TimeSpan, AvailableSlotDto>();

        foreach (var groomer in groomers)
        {
            var groomerSchedules = schedules
                .Where(s => s.GroomerId == groomer.Id);

            foreach (var schedule in groomerSchedules)
            {
                if (schedule.SlotDurationMinutes == 0)
                    continue;

                TimeSpan shiftStart = schedule.StartTime;
                TimeSpan shiftEnd = schedule.EndTime;
                TimeSpan step = TimeSpan.FromMinutes(schedule.SlotDurationMinutes);

                for (TimeSpan start = shiftStart;
                     start + TimeSpan.FromMinutes(duration) <= shiftEnd;
                     start += step)
                {
                    DateTime startDateTime = date + start;
                    DateTime endDateTime = startDateTime.AddMinutes(duration);


                    bool hasTimeOff = timeOffs.Any(t =>
                        t.GroomerId == groomer.Id &&
                        t.StartDatetime < endDateTime &&
                        t.EndDatetime > startDateTime);

                    bool hasAppointment = appointments.Any(a =>
                        a.GroomerId == groomer.Id &&
                        a.StartDatetime < endDateTime &&
                        a.EndDatetime > startDateTime);

                    if (hasTimeOff || hasAppointment)
                        continue;

                    if (!result.TryGetValue(start, out var slot))
                    {
                        slot = new AvailableSlotDto
                        {
                            Time = start.ToString(@"hh\:mm")
                        };

                        result.Add(start, slot);
                    }

                    if (!slot.Groomers.Any(g => g.Id == groomer.Id))
                    {
                        slot.Groomers.Add(new AvailableGroomerDto
                        {
                            Id = groomer.Id,
                            Name = groomer.Name
                        });
                    }
                }
            }
        }

        return result
            .OrderBy(x => x.Key)
            .Select(x => x.Value)
            .ToList();
    }
}

public class AvailableSlotDto
{
    public string Time { get; set; } = "";
    public List<AvailableGroomerDto> Groomers { get; set; } = new();
}

public class AvailableGroomerDto
{
    public uint Id { get; set; }
    public string Name { get; set; } = "";
}
