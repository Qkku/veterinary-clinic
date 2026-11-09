
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GroomingSalon.API.Data;
using GroomingSalon.API.Models;

namespace GroomingSalon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroomersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public GroomersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Получить всех мастеров
    [HttpGet]
    public async Task<IActionResult> GetGroomers()
    {
        var connection = _context.Database.GetDbConnection();

        var count = await _context.Groomers.CountAsync();

        var groomers = await _context.Groomers
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
            .ToListAsync();

        return Ok(new
        {
            Database = connection.Database,
            Server = connection.DataSource,
            Count = count,
            Groomers = groomers
        });
    }

    // Получить мастера по ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetGroomer(uint id)
    {
        var groomer = await _context.Groomers
            .Where(g => g.Id == id)
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
            return NotFound();

        return Ok(groomer);
    }

    // Добавить мастера
    [HttpPost]
    public async Task<IActionResult> CreateGroomer(Groomer groomer)
    {
        groomer.Id = 0;
        groomer.CreatedAt = DateTime.UtcNow;

        _context.Groomers.Add(groomer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetGroomer),
            new { id = groomer.Id },
            new { id = groomer.Id, groomer.Name }
        );
    }

    // Изменить мастера
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGroomer(uint id, Groomer updatedGroomer)
    {
        var groomer = await _context.Groomers.FindAsync(id);

        if (groomer == null)
            return NotFound();

        groomer.Name = updatedGroomer.Name;
        groomer.Email = updatedGroomer.Email;
        groomer.Phone = updatedGroomer.Phone;
        groomer.Age = updatedGroomer.Age;
        groomer.Education = updatedGroomer.Education;
        groomer.ExperienceYears = updatedGroomer.ExperienceYears;
        groomer.Specialization = updatedGroomer.Specialization;
        groomer.IsActive = updatedGroomer.IsActive;

        if (!string.IsNullOrWhiteSpace(updatedGroomer.PasswordHash))
            groomer.PasswordHash = updatedGroomer.PasswordHash;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            groomer.Id,
            groomer.Name,
            groomer.Email,
            groomer.Phone,
            groomer.Age,
            groomer.Education,
            groomer.ExperienceYears,
            groomer.Specialization,
            groomer.IsActive,
            groomer.CreatedAt
        });
    }

    // Удалить мастера
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGroomer(uint id)
    {
        var groomer = await _context.Groomers.FindAsync(id);

        if (groomer == null)
            return NotFound();

        _context.Groomers.Remove(groomer);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
