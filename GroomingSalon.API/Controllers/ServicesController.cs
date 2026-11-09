
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GroomingSalon.API.Data;
using GroomingSalon.API.Models;

namespace GroomingSalon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ServicesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Получить все услуги
    [HttpGet]
    public async Task<IActionResult> GetServices()
    {
        var services = await _context.Services.ToListAsync();
        return Ok(services);
    }

    // Получить услугу по ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetService(uint id)
    {
        var service = await _context.Services.FindAsync(id);

        if (service == null)
            return NotFound();

        return Ok(service);
    }

    // Добавить новую услугу
    [HttpPost]
    public async Task<IActionResult> CreateService(Service service)
    {
        service.Id = 0;
        service.CreatedAt = DateTime.UtcNow;

        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetService),
            new { id = service.Id },
            service
        );
    }

    // Изменить существующую услугу
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateService(uint id, Service updatedService)
    {
        var service = await _context.Services.FindAsync(id);

        if (service == null)
            return NotFound();

        service.Name = updatedService.Name;
        service.Description = updatedService.Description;
        service.Price = updatedService.Price;
        service.DurationMinutes = updatedService.DurationMinutes;
        service.IsActive = updatedService.IsActive;

        await _context.SaveChangesAsync();

        return Ok(service);
    }

    // Удалить услугу
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteService(uint id)
    {
        var service = await _context.Services.FindAsync(id);

        if (service == null)
            return NotFound();

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
