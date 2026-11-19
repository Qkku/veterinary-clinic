
using GroomingSalon.API.Data;
using GroomingSalon.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroomingSalon.API.Controllers;

[ApiController]
[Route("api/AdminGroomerSchedules")]
[Authorize(Roles = "Admin")]
public class AdminGroomerSchedulesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminGroomerSchedulesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Получить расписание мастера
    [HttpGet("{groomerId}")]
    public async Task<IActionResult> GetSchedule(uint groomerId)
    {
        var exists = await _context.Groomers
            .AnyAsync(g => g.Id == groomerId);

        if (!exists)
            return NotFound("Мастер не найден.");

        var schedules = await _context.GroomerSchedules
            .Where(s => s.GroomerId == groomerId)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .Select(s => new
            {
                s.Id,
                s.DayOfWeek,
                s.StartTime,
                s.EndTime,
                s.ValidFrom,
                s.ValidTo,
                s.SlotDurationMinutes,
                s.IsActive
            })
            .ToListAsync();

        return Ok(schedules);
    }

    // Полностью заменить расписание мастера
    [HttpPut("{groomerId}")]
    public async Task<IActionResult> SetSchedule(
        uint groomerId,
        SetGroomerScheduleRequest request)
    {
        var groomer = await _context.Groomers
            .FirstOrDefaultAsync(g => g.Id == groomerId);

        if (groomer == null)
            return NotFound("Мастер не найден.");

        if (groomer.IsActive != true)
            return BadRequest("Нельзя установить расписание неактивному мастеру.");

        DateTime validFrom = request.ValidFrom.Date;

        if (request.ValidTo.HasValue &&
            request.ValidTo.Value.Date < validFrom)
            return BadRequest("Дата окончания раньше даты начала.");

        if (request.SlotDurationMinutes == 0)
            return BadRequest("Продолжительность слота должна быть больше нуля.");

        if (request.Days.Select(d => d.DayOfWeek).Distinct().Count()
            != request.Days.Count)
            return BadRequest("Дни недели не должны повторяться.");

        foreach (var day in request.Days)
        {
            if (day.DayOfWeek < 1 || day.DayOfWeek > 7)
                return BadRequest("День недели должен быть от 1 до 7.");

            var intervals = day.Intervals
                .OrderBy(i => i.StartTime)
                .ToList();

            for (int i = 0; i < intervals.Count; i++)
            {
                var interval = intervals[i];

                if (interval.StartTime < TimeSpan.Zero ||
                    interval.EndTime > TimeSpan.FromHours(24) ||
                    interval.StartTime >= interval.EndTime)
                    return BadRequest("Некорректное время рабочего интервала.");

                if (i > 0 &&
                    intervals[i - 1].EndTime > interval.StartTime)
                    return BadRequest("Рабочие интервалы не должны пересекаться.");
            }
        }

        // Завершаем прежний график перед датой нового
        var oldSchedules = await _context.GroomerSchedules
            .Where(s =>
                s.GroomerId == groomerId &&
                s.IsActive == true &&
                s.ValidFrom < validFrom &&
                (s.ValidTo == null || s.ValidTo >= validFrom))
            .ToListAsync();

        foreach (var schedule in oldSchedules)
            schedule.ValidTo = validFrom.AddDays(-1);

        // Удаляем будущие интервалы прежнего графика
        var futureSchedules = await _context.GroomerSchedules
            .Where(s =>
                s.GroomerId == groomerId &&
                s.ValidFrom >= validFrom)
            .ToListAsync();

        _context.GroomerSchedules.RemoveRange(futureSchedules);

        // Добавляем новые интервалы
        foreach (var day in request.Days)
        {
            foreach (var interval in day.Intervals)
            {
                _context.GroomerSchedules.Add(new GroomerSchedule
                {
                    GroomerId = groomerId,
                    DayOfWeek = day.DayOfWeek,
                    StartTime = interval.StartTime,
                    EndTime = interval.EndTime,
                    ValidFrom = validFrom,
                    ValidTo = request.ValidTo?.Date,
                    SlotDurationMinutes = request.SlotDurationMinutes,
                    IsActive = true
                });
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Расписание обновлено.",
            groomerId,
            validFrom,
            request.ValidTo,
            intervalsCount = request.Days.Sum(d => d.Intervals.Count)
        });
    }
}

public class SetGroomerScheduleRequest
{
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public ushort SlotDurationMinutes { get; set; } = 30;
    public List<GroomerScheduleDayDto> Days { get; set; } = new();
}

public class GroomerScheduleDayDto
{
    public sbyte DayOfWeek { get; set; }
    public List<GroomerScheduleIntervalDto> Intervals { get; set; } = new();
}

public class GroomerScheduleIntervalDto
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
