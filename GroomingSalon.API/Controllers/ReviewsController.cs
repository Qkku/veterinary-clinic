
using GroomingSalon.API.Data;
using GroomingSalon.API.Models;
using GroomingSalon.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GroomingSalon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly NotificationService _notifications;

    public ReviewsController(ApplicationDbContext context, NotificationService notifications)
    {
        _context = context;
        _notifications = notifications;

    }

    private bool TryGetUserId(out uint userId) =>
        uint.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    // Получить отзывы текущего пользователя
    [HttpGet]
    public async Task<IActionResult> GetReviews()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var reviews = await _context.Reviews
            .Where(r => r.UserId == userId)
            .Select(r => new
            {
                r.Id,
                r.UserId,
                UserName = r.User.Name,
                r.GroomerId,
                GroomerName = r.Groomer.Name,
                r.AppointmentId,
                r.Rating,
                r.Text,
                r.CreatedAt
            })
            .ToListAsync();

        return Ok(reviews);
    }

    // Получить конкретный отзыв
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReview(uint id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var review = await _context.Reviews
            .Where(r => r.Id == id && r.UserId == userId)
            .Select(r => new
            {
                r.Id,
                r.UserId,
                UserName = r.User.Name,
                r.GroomerId,
                GroomerName = r.Groomer.Name,
                r.AppointmentId,
                r.Rating,
                r.Text,
                r.CreatedAt
            })
            .FirstOrDefaultAsync();

        return review == null ? NotFound() : Ok(review);
    }

    // Создать отзыв
    [HttpPost]
    public async Task<IActionResult> CreateReview(ReviewCreateDto dto)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        if (dto.Rating < 1 || dto.Rating > 5)
            return BadRequest("Оценка должна быть от 1 до 5.");

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a =>
                a.Id == dto.AppointmentId &&
                a.UserId == userId);

        if (appointment == null)
            return BadRequest("Запись не найдена.");

        if (appointment.Status != "completed")
            return BadRequest("Оставить отзыв можно только после завершения услуги.");

        if (await _context.Reviews.AnyAsync(r =>
            r.AppointmentId == appointment.Id))
        {
            return BadRequest("Для этой записи отзыв уже существует.");
        }

        var review = new Review
        {
            UserId = userId,
            GroomerId = appointment.GroomerId,
            AppointmentId = appointment.Id,
            Rating = dto.Rating,
            Text = dto.Text,
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        await _notifications.NotifyGroomerAsync(
            review.GroomerId,
            "Новый отзыв",
            $"Клиент оставил отзыв. Оценка: {review.Rating}/5.",
            "review_created");

        return CreatedAtAction(
            nameof(GetReview),
            new { id = review.Id },
            new { id = review.Id });
    }

    // Редактировать отзыв
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReview(
        uint id, ReviewUpdateDto dto)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (review == null)
            return NotFound();

        if (dto.Rating < 1 || dto.Rating > 5)
            return BadRequest("Оценка должна быть от 1 до 5.");

        review.Rating = dto.Rating;
        review.Text = dto.Text;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Отзыв обновлён." });
    }

    // Удалить отзыв
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(uint id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (review == null)
            return NotFound();

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class ReviewCreateDto
{
    [Required]
    public uint AppointmentId { get; set; }

    [Range(1, 5)]
    public byte Rating { get; set; }

    public string? Text { get; set; }
}

public class ReviewUpdateDto
{
    [Range(1, 5)]
    public byte Rating { get; set; }

    public string? Text { get; set; }
}
