
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

    public AppointmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private bool TryGetUserId(out uint userId) =>
        uint.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out userId);

    // Получение записей текущего пользователя
    [HttpGet]
    public async Task<IActionResult> GetAppointments()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var appointments = await _context.Appointments
            .Where(a => a.UserId == userId)
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

    // Получение одной записи пользователя
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAppointment(uint id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var appointment = await _context.Appointments
            .Where(a => a.Id == id && a.UserId == userId)
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

        return appointment == null ? NotFound() : Ok(appointment);
    }

    // Получение всех записей салона (только администратор)
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllAppointments()
    {
        var appointments = await _context.Appointments
            .OrderBy(a => a.StartDatetime)
            .Select(a => new
            {
                a.Id,
                a.UserId,
                CustomerName = a.User.Name,
                CustomerPhone = a.User.Phone,
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

    // Изменение статуса записи (только администратор)
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAppointmentStatus(
        uint id,
        [FromBody] UpdateAppointmentStatusRequest request)
    {
        var allowedStatuses = new[]
        {
        "booked",
        "confirmed",
        "completed",
        "cancelled",
        "no_show"
    };

        if (!allowedStatuses.Contains(request.Status))
            return BadRequest("Недопустимый статус записи.");

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
            return NotFound("Запись не найдена.");

        appointment.Status = request.Status;

        bool accountBanned = false;

        if (request.Status == "no_show")
        {
            // Получаем три последние записи клиента,
            // которые уже начались
            var recentAppointments = await _context.Appointments
                .Where(a =>
                    a.UserId == appointment.UserId &&
                    a.StartDatetime <= DateTime.Now)
                .OrderByDescending(a => a.StartDatetime)
                .ThenByDescending(a => a.Id)
                .Take(3)
                .ToListAsync();

            // Проверяем, все ли три записи имеют статус no_show
            if (recentAppointments.Count == 3 &&
                recentAppointments.All(a => a.Status == "no_show"))
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == appointment.UserId);

                if (user != null)
                {
                    user.IsBanned = true;
                    accountBanned = true;

                    // Отменяем все будущие активные записи клиента
                    var futureAppointments = await _context.Appointments
                        .Where(a =>
                            a.UserId == appointment.UserId &&
                            a.StartDatetime > DateTime.Now &&
                            (a.Status == "booked" ||
                             a.Status == "confirmed"))
                        .ToListAsync();

                    foreach (var futureAppointment in futureAppointments)
                    {
                        futureAppointment.Status = "cancelled";
                    }
                }
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            appointment.Id,
            appointment.Status,
            AccountBanned = accountBanned
        });
    }

    // Создание записи
    [HttpPost]
    public async Task<IActionResult> CreateAppointment(Appointment appointment)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        DateTime start = appointment.StartDatetime;
        DateTime now = DateTime.Now;

        if (start <= now)
            return BadRequest("Нельзя создать запись на прошедшее время.");

        if (start > now.AddMonths(3))
            return BadRequest("Запись доступна максимум на 3 месяца вперёд.");

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE groomers SET id = id WHERE id = {appointment.GroomerId}");

            var groomer = await _context.Groomers.FirstOrDefaultAsync(g =>
                g.Id == appointment.GroomerId && g.IsActive == true);

            if (groomer == null)
                return BadRequest("Мастер не найден или неактивен.");

            var pet = await _context.Pets.FirstOrDefaultAsync(p =>
                p.Id == appointment.PetId && p.UserId == userId);

            if (pet == null)
                return BadRequest("Питомец не найден или принадлежит другому пользователю.");

            var service = await _context.Services.FirstOrDefaultAsync(s =>
                s.Id == appointment.ServiceId && s.IsActive == true);

            if (service == null)
                return BadRequest("Услуга не найдена или неактивна.");

            DateTime end = start.AddMinutes(service.DurationMinutes);

            sbyte dayOfWeek =
                (sbyte)(((int)start.DayOfWeek + 6) % 7 + 1);

            var schedules = await _context.GroomerSchedules
                .Where(s =>
                    s.GroomerId == groomer.Id &&
                    s.DayOfWeek == dayOfWeek &&
                    s.IsActive == true &&
                    s.ValidFrom <= start.Date &&
                    (s.ValidTo == null || s.ValidTo >= start.Date))
                .ToListAsync();

            TimeSpan startTime = start.TimeOfDay;
            TimeSpan endTime = end.TimeOfDay;

            bool fitsSchedule = schedules.Any(s =>
                startTime >= s.StartTime &&
                endTime <= s.EndTime &&
                s.SlotDurationMinutes > 0 &&
                (startTime - s.StartTime).TotalMinutes %
                    s.SlotDurationMinutes == 0);

            if (!fitsSchedule)
                return BadRequest("Выбранное время не соответствует расписанию мастера.");

            bool hasTimeOff = await _context.GroomerTimeOffs.AnyAsync(t =>
                t.GroomerId == groomer.Id &&
                t.StartDatetime < end &&
                t.EndDatetime > start);

            if (hasTimeOff)
                return Conflict("Мастер отсутствует в выбранное время.");

            bool hasAppointment = await _context.Appointments.AnyAsync(a =>
                a.GroomerId == groomer.Id &&
                (a.Status == "booked" || a.Status == "confirmed") &&
                a.StartDatetime < end &&
                a.EndDatetime > start);

            if (hasAppointment)
                return Conflict("Это время уже занято. Выберите другой слот.");

            appointment.Id = 0;
            appointment.UserId = userId;
            appointment.StartDatetime = start;
            appointment.EndDatetime = end;
            appointment.CreatedAt = DateTime.UtcNow;
            appointment.Status = "booked";

            _context.Appointments.Add(appointment);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return CreatedAtAction(
                nameof(GetAppointment),
                new { id = appointment.Id },
                new { id = appointment.Id });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Отмена записи пользователем
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletesAppointment(uint id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a =>
                a.Id == id && a.UserId == userId);

        if (appointment == null)
            return NotFound("Запись не найдена.");

        if (appointment.Status != "booked" &&
            appointment.Status != "confirmed")
            return Conflict("Эту запись нельзя отменить.");

        if (appointment.StartDatetime <= DateTime.Now)
            return BadRequest("Нельзя отменить запись, время которой уже наступило.");

        appointment.Status = "cancelled";

        await _context.SaveChangesAsync();

        return NoContent();
    }
}


public class UpdateAppointmentStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
