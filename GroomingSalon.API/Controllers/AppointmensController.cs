using System.Security.Claims;
using GroomingSalon.API.Data;
using GroomingSalon.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroomingSalon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public AppointmentsController(ApplicationDbContext context) => _context = context;

    private bool TryGetUserId(out uint userId) =>
        uint.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    [HttpGet]
    public async Task<IActionResult> GetAppointments()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var appointments = await _context.Appointments.Where(a => a.UserId == userId)
            .Select(a => new
            {
                a.Id, a.UserId, a.PetId, PetName = a.Pet.Name,
                a.GroomerId, GroomerName = a.Groomer.Name,
                a.ServiceId, ServiceName = a.Service.Name,
                a.StartDatetime, a.EndDatetime, a.Status, a.Comment, a.CreatedAt
            }).ToListAsync();
        return Ok(appointments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointment(uint id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var appointment = await _context.Appointments
            .Where(a => a.Id == id && a.UserId == userId)
            .Select(a => new
            {
                a.Id, a.UserId, a.PetId, PetName = a.Pet.Name,
                a.GroomerId, GroomerName = a.Groomer.Name,
                a.ServiceId, ServiceName = a.Service.Name,
                a.StartDatetime, a.EndDatetime, a.Status, a.Comment, a.CreatedAt
            }).FirstOrDefaultAsync();
        return appointment == null ? NotFound() : Ok(appointment);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppointment(Appointment appointment)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var pet = await _context.Pets.FirstOrDefaultAsync(p =>
            p.Id == appointment.PetId && p.UserId == userId);
        if (pet == null)
            return BadRequest("Питомец не найден или принадлежит другому пользователю.");

        var groomer = await _context.Groomers.FirstOrDefaultAsync(g =>
            g.Id == appointment.GroomerId && g.IsActive == true);
        if (groomer == null) return BadRequest("Мастер не найден или неактивен.");

        var service = await _context.Services.FirstOrDefaultAsync(s =>
            s.Id == appointment.ServiceId && s.IsActive == true);
        if (service == null) return BadRequest("Услуга не найдена или неактивна.");

        if (appointment.StartDatetime <= DateTime.Now)
            return BadRequest("Нельзя создать запись на прошедшее время.");

        appointment.Id = 0;
        appointment.UserId = userId; // Нельзя записать от имени другого пользователя.
        appointment.EndDatetime = appointment.StartDatetime.AddMinutes(service.DurationMinutes);
        appointment.CreatedAt = DateTime.UtcNow;
        appointment.Status = "booked"; // Допустимое значение enum в MySQL.

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, new { id = appointment.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointment(uint id, Appointment updatedAppointment)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (appointment == null) return NotFound();

        var pet = await _context.Pets.FirstOrDefaultAsync(p =>
            p.Id == updatedAppointment.PetId && p.UserId == userId);
        if (pet == null) return BadRequest("Питомец не найден или принадлежит другому пользователю.");

        var groomer = await _context.Groomers.FirstOrDefaultAsync(g =>
            g.Id == updatedAppointment.GroomerId && g.IsActive == true);
        if (groomer == null) return BadRequest("Мастер не найден или неактивен.");

        var service = await _context.Services.FirstOrDefaultAsync(s =>
            s.Id == updatedAppointment.ServiceId && s.IsActive == true);
        if (service == null) return BadRequest("Услуга не найдена или неактивна.");
        if (updatedAppointment.StartDatetime <= DateTime.Now)
            return BadRequest("Нельзя перенести запись на прошедшее время.");

        appointment.PetId = updatedAppointment.PetId;
        appointment.GroomerId = updatedAppointment.GroomerId;
        appointment.ServiceId = updatedAppointment.ServiceId;
        appointment.StartDatetime = updatedAppointment.StartDatetime;
        appointment.EndDatetime = updatedAppointment.StartDatetime.AddMinutes(service.DurationMinutes);
        appointment.Comment = updatedAppointment.Comment;
        // Статус и UserId клиент менять не может.

        await _context.SaveChangesAsync();
        return Ok(new { message = "Запись обновлена." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppointment(uint id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (appointment == null) return NotFound();
        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
