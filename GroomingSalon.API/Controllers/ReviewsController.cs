
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GroomingSalon.API.Data;
using GroomingSalon.API.Models;

namespace GroomingSalon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReviewsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Получить все отзывы
    [HttpGet]
    public async Task<IActionResult> GetReviews()
    {
        var reviews = await _context.Reviews
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

    // Получить отзыв по ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReview(uint id)
    {
        var review = await _context.Reviews
            .Where(r => r.Id == id)
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

        if (review == null)
            return NotFound();

        return Ok(review);
    }

    // Создать отзыв
    [HttpPost]
    public async Task<IActionResult> CreateReview(Review review)
    {
        if (review.Rating < 1 || review.Rating > 5)
            return BadRequest("Оценка должна быть от 1 до 5.");

        var userExists = await _context.Users
            .AnyAsync(u => u.Id == review.UserId);

        if (!userExists)
            return BadRequest("Пользователь не найден.");

        var groomerExists = await _context.Groomers
            .AnyAsync(g => g.Id == review.GroomerId);

        if (!groomerExists)
            return BadRequest("Мастер не найден.");

        if (review.AppointmentId.HasValue)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a =>
                    a.Id == review.AppointmentId.Value &&
                    a.UserId == review.UserId &&
                    a.GroomerId == review.GroomerId);

            if (appointment == null)
                return BadRequest("Запись не найдена или не соответствует пользователю и мастеру.");

            var reviewExists = await _context.Reviews
                .AnyAsync(r => r.AppointmentId == review.AppointmentId);

            if (reviewExists)
                return BadRequest("Для этой записи отзыв уже существует.");
        }

        review.Id = 0;
        review.CreatedAt = DateTime.UtcNow;

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetReview),
            new { id = review.Id },
            new { id = review.Id }
        );
    }

    // Изменить отзыв
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReview(uint id, Review updatedReview)
    {
        var review = await _context.Reviews.FindAsync(id);

        if (review == null)
            return NotFound();

        if (updatedReview.Rating < 1 || updatedReview.Rating > 5)
            return BadRequest("Оценка должна быть от 1 до 5.");

        review.Rating = updatedReview.Rating;
        review.Text = updatedReview.Text;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Отзыв обновлён." });
    }

    // Удалить отзыв
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(uint id)
    {
        var review = await _context.Reviews.FindAsync(id);

        if (review == null)
            return NotFound();

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
