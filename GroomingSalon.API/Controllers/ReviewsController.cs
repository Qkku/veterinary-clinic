
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
            Status = "pending",
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
        Review? review;

        if (User.IsInRole("Admin"))
        {

            review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        else
        {

            if (!TryGetUserId(out var userId))
                return Unauthorized();

            review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
        }

        if (review == null)
            return NotFound();

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Публичные отзывы, прошедшие модерацию
    [AllowAnonymous]
    [HttpGet("public")]
    public async Task<IActionResult> GetPublicReviews()
    {
        var rows = await _context.Reviews.Where(r => r.Status == "approved")
            .OrderByDescending(r => r.CreatedAt).Select(r => new { r.Id, r.Rating, r.Text, r.CreatedAt, r.GroomerId, GroomerName = r.Groomer.Name, UserName = r.User.Name }).ToListAsync();
        return Ok(rows);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllReviews()
    {
        var rows = await _context.Reviews.OrderByDescending(r => r.CreatedAt)
            .Select(r => new { r.Id, r.UserId, UserName = r.User.Name, r.GroomerId, GroomerName = r.Groomer.Name, r.Rating, r.Text, r.Status, r.CreatedAt }).ToListAsync();
        return Ok(rows);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingReviews()
    {
        var rows = await _context.Reviews.Where(r => r.Status == "pending").OrderBy(r => r.CreatedAt)
            .Select(r => new { r.Id, r.UserId, UserName = r.User.Name, r.GroomerId, GroomerName = r.Groomer.Name, r.Rating, r.Text, r.Status, r.CreatedAt }).ToListAsync();
        return Ok(rows);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/moderate")]
    public async Task<IActionResult> Moderate(uint id, [FromBody] ReviewModerationDto dto)
    {
        if (dto.Status is not ("approved" or "rejected")) return BadRequest(new { message = "Допустимые статусы: approved, rejected." });
        var review = await _context.Reviews.FindAsync(id);
        if (review == null) return NotFound();
        review.Status = dto.Status;
        await _context.SaveChangesAsync();
        return Ok(new { review.Id, review.Status });
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

public class ReviewModerationDto { [Required] public string Status { get; set; } = ""; }
