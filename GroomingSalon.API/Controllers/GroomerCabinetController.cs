
using System.Security.Claims;
using GroomingSalon.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroomingSalon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Groomer")]
public class GroomerCabinetController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public GroomerCabinetController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Профиль текущего мастера
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!uint.TryParse(idValue, out var groomerId))
            return Unauthorized();

        var groomer = await _context.Groomers
            .Where(g => g.Id == groomerId)
            .Select(g => new
            {
                g.Id,
                g.Name,
                g.Email,
                g.Phone,
                g.Age,
                g.Education,
                g.ExperienceYears,
                g.Specialization,
                g.IsActive,
                g.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (groomer == null)
            return NotFound(new { message = "Мастер не найден." });

        return Ok(groomer);
    }


    [HttpGet("schedule")]
    public async Task<IActionResult> GetSchedule([FromQuery] DateTime? date)
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!uint.TryParse(idValue, out var groomerId))
            return Unauthorized();


        DateTime selectedDate = (date ?? DateTime.Today).Date;

        int daysFromMonday =
            ((int)selectedDate.DayOfWeek + 6) % 7;

        DateTime weekStart = selectedDate.AddDays(-daysFromMonday);
        DateTime weekEnd = weekStart.AddDays(7);


        var schedules = await _context.GroomerSchedules
            .Where(s =>
                s.GroomerId == groomerId &&
                s.IsActive == true &&
                s.ValidFrom < weekEnd &&
                (s.ValidTo == null || s.ValidTo >= weekStart))
            .ToListAsync();


        var timeOffs = await _context.GroomerTimeOffs
            .Where(t =>
                t.GroomerId == groomerId &&
                t.StartDatetime < weekEnd &&
                t.EndDatetime > weekStart)
            .OrderBy(t => t.StartDatetime)
            .ToListAsync();


        var appointments = await _context.Appointments
            .Where(a =>
                a.GroomerId == groomerId &&
                a.StartDatetime < weekEnd &&
                a.EndDatetime > weekStart)
            .Include(a => a.User)
            .Include(a => a.Pet)
            .Include(a => a.Service)
            .OrderBy(a => a.StartDatetime)
            .ToListAsync();

        var days = new List<object>();


        for (int i = 0; i < 7; i++)
        {
            DateTime currentDate = weekStart.AddDays(i);

            int dayOfWeek = ((int)currentDate.DayOfWeek + 6) % 7 + 1;

  
            var daySchedules = schedules
                .Where(s =>
                    s.DayOfWeek == dayOfWeek &&
                    s.ValidFrom.Date <= currentDate &&
                    (s.ValidTo == null ||
                     s.ValidTo.Value.Date >= currentDate))
                .Select(s => new
                {
                    s.Id,
                    StartTime = s.StartTime.ToString(@"hh\:mm"),
                    EndTime = s.EndTime.ToString(@"hh\:mm"),
                    s.SlotDurationMinutes
                })
                .ToList();


            var dayTimeOffs = timeOffs
                .Where(t =>
                    t.StartDatetime < currentDate.AddDays(1) &&
                    t.EndDatetime > currentDate)
                .Select(t => new
                {
                    t.Id,
                    t.StartDatetime,
                    t.EndDatetime,
                    t.Type,
                    t.Comment
                })
                .ToList();


            var dayAppointments = appointments
                .Where(a =>
                    a.StartDatetime < currentDate.AddDays(1) &&
                    a.EndDatetime > currentDate)
                .Select(a => new
                {
                    a.Id,

                    a.StartDatetime,
                    a.EndDatetime,

                    a.Status,
                    a.Comment,

                    Client = new
                    {
                        a.User.Id,
                        a.User.Name,
                        a.User.Phone
                    },

                    Pet = new
                    {
                        a.Pet.Id,
                        a.Pet.Name,
                        a.Pet.Age,
                        a.Pet.Breed,
                        a.Pet.Size,
                        a.Pet.Notes
                    },

                    Service = new
                    {
                        a.Service.Id,
                        a.Service.Name,
                        a.Service.DurationMinutes,
                        a.Service.Price
                    }
                })
                .ToList();

            days.Add(new
            {
                Date = currentDate.ToString("yyyy-MM-dd"),
                DayOfWeek = dayOfWeek,

                Schedules = daySchedules,
                TimeOffs = dayTimeOffs,
                Appointments = dayAppointments
            });
        }

        return Ok(new
        {
            WeekStart = weekStart.ToString("yyyy-MM-dd"),
            WeekEnd = weekEnd.AddDays(-1).ToString("yyyy-MM-dd"),
            Days = days
        });
    }
    [HttpGet("reviews")]
    public async Task<IActionResult> GetReviews()
    {
        if (!uint.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var groomerId)) return Unauthorized();
        var rows = await _context.Reviews.Where(r => r.GroomerId == groomerId && r.Status == "approved")
            .OrderByDescending(r => r.CreatedAt).Select(r => new { r.Id, r.Rating, r.Text, r.CreatedAt, UserName = r.User.Name }).ToListAsync();
        return Ok(rows);
    }

}
