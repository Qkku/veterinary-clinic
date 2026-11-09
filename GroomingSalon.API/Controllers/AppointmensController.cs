
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GroomingSalon.API.Data;
using GroomingSalon.API.Models;

namespace GroomingSalon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AppointmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Получить все записи
    [HttpGet]
    public async Task<IActionResult> GetAppointments()
    {
        var appointments = await _context.Appointments
            .Select(a => new
            {
                a.Id,
                a.UserId,
                a.PetId,
                PetName = a.Pet.Name,
                a.GroomerId,
                GroomerName = a.Groomer.Name,
                a.ServiceId,
                ServiceName = a.Service.Name,
                a.StartDatetime,
                a.EndDatetime,
                a.Status,
                a.Comment,
                a.CreatedAt
            })
            .ToListAsync();

        return Ok(appointments);
    }

    // Получить запись по ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointment(uint id)
    {
        var appointment = await _context.Appointments
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id,
                a.UserId,
                a.PetId,
                PetName = a.Pet.Name,
                a.GroomerId,
                GroomerName = a.Groomer.Name,
                a.ServiceId,
                ServiceName = a.Service.Name,
                a.StartDatetime,
                a.EndDatetime,
                a.Status,
                a.Comment,
                a.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (appointment == null)
            return NotFound();

        return Ok(appointment);
    }

    // Создать запись
    [HttpPost]
    public async Task<IActionResult> CreateAppointment(Appointment appointment)
    {
        // Проверяем пользователя
        var userExists = await _context.Users
            .AnyAsync(u => u.Id == appointment.UserId);

        if (!userExists)
            return BadRequest("Пользователь не найден.");

        // Проверяем питомца и его владельца
        var pet = await _context.Pets
            .FirstOrDefaultAsync(p =>
                p.Id == appointment.PetId &&
                p.UserId == appointment.UserId);

        if (pet == null)
            return BadRequest("Питомец не найден или принадлежит другому пользователю.");

        // Проверяем мастера
        var groomer = await _context.Groomers
            .FirstOrDefaultAsync(g =>
                g.Id == appointment.GroomerId &&
                g.IsActive == true);

        if (groomer == null)
            return BadRequest("Мастер не найден или неактивен.");

        // Проверяем услугу
        var service = await _context.Services
            .FirstOrDefaultAsync(s =>
                s.Id == appointment.ServiceId &&
                s.IsActive == true);

        if (service == null)
            return BadRequest("Услуга не найдена или неактивна.");

        if (appointment.StartDatetime <= DateTime.UtcNow)
            return BadRequest("Нельзя создать запись на прошедшее время.");

        // Продолжительность записи определяется услугой
        appointment.EndDatetime = appointment.StartDatetime
            .AddMinutes(service.DurationMinutes);

        appointment.Id = 0;
        appointment.CreatedAt = DateTime.UtcNow;
        appointment.Status = "pending";

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetAppointment),
            new { id = appointment.Id },
            new { id = appointment.Id }
        );
    }

    // Изменить запись
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAppointment(
        uint id,
        Appointment updatedAppointment)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null)
            return NotFound();

        var pet = await _context.Pets.FirstOrDefaultAsync(p =>
            p.Id == updatedAppointment.PetId &&
            p.UserId == updatedAppointment.UserId);

        if (pet == null)
            return BadRequest("Питомец не найден или принадлежит другому пользователю.");

        var groomer = await _context.Groomers.FirstOrDefaultAsync(g =>
            g.Id == updatedAppointment.GroomerId &&
            g.IsActive == true);

        if (groomer == null)
            return BadRequest("Мастер не найден или неактивен.");

        var service = await _context.Services.FirstOrDefaultAsync(s =>
            s.Id == updatedAppointment.ServiceId &&
            s.IsActive == true);

        if (service == null)
            return BadRequest("Услуга не найдена или неактивна.");

        if (updatedAppointment.StartDatetime <= DateTime.UtcNow)
            return BadRequest("Нельзя перенести запись на прошедшее время.");

        appointment.UserId = updatedAppointment.UserId;
        appointment.PetId = updatedAppointment.PetId;
        appointment.GroomerId = updatedAppointment.GroomerId;
        appointment.ServiceId = updatedAppointment.ServiceId;
        appointment.StartDatetime = updatedAppointment.StartDatetime;
        appointment.EndDatetime = updatedAppointment.StartDatetime
            .AddMinutes(service.DurationMinutes);
        appointment.Status = updatedAppointment.Status;
        appointment.Comment = updatedAppointment.Comment;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Запись обновлена." });
    }

    // Удалить запись
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAppointment(uint id)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment == null)
            return NotFound();

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
