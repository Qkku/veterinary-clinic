using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GroomingSalon.API.Data;
using GroomingSalon.API.Models;

namespace GroomingSalon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroomerTimeOffController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public GroomerTimeOffController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Получить все периоды отсутствия
    [HttpGet]
    public async Task<IActionResult> GetTimeOffs()
    {
        var timeOffs = await _context.GroomerTimeOffs
            .Select(t => new
            {
                t.Id,
                t.GroomerId,
                GroomerName = t.Groomer.Name,
                t.StartDatetime,
                t.EndDatetime,
                t.Type,
                t.Comment
            })
            .ToListAsync();

        return Ok(timeOffs);
    }

    // Получить период отсутствия по ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTimeOff(uint id)
    {
        var timeOff = await _context.GroomerTimeOffs
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.GroomerId,
                GroomerName = t.Groomer.Name,
                t.StartDatetime,
                t.EndDatetime,
                t.Type,
                t.Comment
            })
            .FirstOrDefaultAsync();

        if (timeOff == null)
            return NotFound();

        return Ok(timeOff);
    }

    // Добавить период отсутствия
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateTimeOff(GroomerTimeOff timeOff)
    {
        var groomerExists = await _context.Groomers
            .AnyAsync(g => g.Id == timeOff.GroomerId);

        if (!groomerExists)
            return BadRequest("Мастер не найден.");

        if (timeOff.StartDatetime >= timeOff.EndDatetime)
            return BadRequest("Дата окончания должна быть позже даты начала.");

        timeOff.Id = 0;

        _context.GroomerTimeOffs.Add(timeOff);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetTimeOff),
            new { id = timeOff.Id },
            new { id = timeOff.Id }
        );
    }

    // Изменить период отсутствия
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTimeOff(
        uint id,
        GroomerTimeOff updatedTimeOff)
    {
        var timeOff = await _context.GroomerTimeOffs.FindAsync(id);

        if (timeOff == null)
            return NotFound();

        var groomerExists = await _context.Groomers
            .AnyAsync(g => g.Id == updatedTimeOff.GroomerId);

        if (!groomerExists)
            return BadRequest("Мастер не найден.");

        if (updatedTimeOff.StartDatetime >= updatedTimeOff.EndDatetime)
            return BadRequest("Дата окончания должна быть позже даты начала.");

        timeOff.GroomerId = updatedTimeOff.GroomerId;
        timeOff.StartDatetime = updatedTimeOff.StartDatetime;
        timeOff.EndDatetime = updatedTimeOff.EndDatetime;
        timeOff.Type = updatedTimeOff.Type;
        timeOff.Comment = updatedTimeOff.Comment;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Период отсутствия обновлён." });
    }

    // Удалить период отсутствия
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTimeOff(uint id)
    {
        var timeOff = await _context.GroomerTimeOffs.FindAsync(id);

        if (timeOff == null)
            return NotFound();

        _context.GroomerTimeOffs.Remove(timeOff);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
