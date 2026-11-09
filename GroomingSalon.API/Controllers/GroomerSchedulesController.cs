
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GroomingSalon.API.Data;
using GroomingSalon.API.Models;

namespace GroomingSalon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroomerSchedulesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public GroomerSchedulesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Получить всё расписание
    [HttpGet]
    public async Task<IActionResult> GetSchedules()
    {
        var schedules = await _context.GroomerSchedules
            .Select(s => new
            {
                s.Id,
                s.GroomerId,
                GroomerName = s.Groomer.Name,
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

    // Получить расписание по ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSchedule(uint id)
    {
        var schedule = await _context.GroomerSchedules
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.GroomerId,
                GroomerName = s.Groomer.Name,
                s.DayOfWeek,
                s.StartTime,
                s.EndTime,
                s.ValidFrom,
                s.ValidTo,
                s.SlotDurationMinutes,
                s.IsActive
            })
            .FirstOrDefaultAsync();

        if (schedule == null)
            return NotFound();

        return Ok(schedule);
    }

    // Добавить расписание
    [HttpPost]
    public async Task<IActionResult> CreateSchedule(GroomerSchedule schedule)
    {
        var groomerExists = await _context.Groomers
            .AnyAsync(g => g.Id == schedule.GroomerId);

        if (!groomerExists)
            return BadRequest("Мастер не найден.");

        if (schedule.DayOfWeek < 1 || schedule.DayOfWeek > 7)
            return BadRequest("День недели должен быть от 1 до 7.");

        if (schedule.StartTime >= schedule.EndTime)
            return BadRequest("Время окончания должно быть позже времени начала.");

        if (schedule.SlotDurationMinutes == 0)
            return BadRequest("Продолжительность слота должна быть больше нуля.");

        if (schedule.ValidTo.HasValue &&
            schedule.ValidTo.Value.Date < schedule.ValidFrom.Date)
            return BadRequest("Дата окончания действия расписания не может быть раньше даты начала.");

        schedule.Id = 0;

        _context.GroomerSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetSchedule),
            new { id = schedule.Id },
            new { id = schedule.Id }
        );
    }

    // Изменить расписание
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSchedule(
        uint id,
        GroomerSchedule updatedSchedule)
    {
        var schedule = await _context.GroomerSchedules.FindAsync(id);

        if (schedule == null)
            return NotFound();

        var groomerExists = await _context.Groomers
            .AnyAsync(g => g.Id == updatedSchedule.GroomerId);

        if (!groomerExists)
            return BadRequest("Мастер не найден.");

        if (updatedSchedule.DayOfWeek < 1 || updatedSchedule.DayOfWeek > 7)
            return BadRequest("День недели должен быть от 1 до 7.");

        if (updatedSchedule.StartTime >= updatedSchedule.EndTime)
            return BadRequest("Время окончания должно быть позже времени начала.");

        if (updatedSchedule.SlotDurationMinutes == 0)
            return BadRequest("Продолжительность слота должна быть больше нуля.");

        if (updatedSchedule.ValidTo.HasValue &&
            updatedSchedule.ValidTo.Value.Date < updatedSchedule.ValidFrom.Date)
            return BadRequest("Дата окончания действия расписания не может быть раньше даты начала.");

        schedule.GroomerId = updatedSchedule.GroomerId;
        schedule.DayOfWeek = updatedSchedule.DayOfWeek;
        schedule.StartTime = updatedSchedule.StartTime;
        schedule.EndTime = updatedSchedule.EndTime;
        schedule.ValidFrom = updatedSchedule.ValidFrom;
        schedule.ValidTo = updatedSchedule.ValidTo;
        schedule.SlotDurationMinutes = updatedSchedule.SlotDurationMinutes;
        schedule.IsActive = updatedSchedule.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Расписание обновлено." });
    }

    // Удалить расписание
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSchedule(uint id)
    {
        var schedule = await _context.GroomerSchedules.FindAsync(id);

        if (schedule == null)
            return NotFound();

        _context.GroomerSchedules.Remove(schedule);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
