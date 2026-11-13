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
public class ReviewsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public ReviewsController(ApplicationDbContext context) => _context = context;

    private bool TryGetUserId(out uint userId) =>
        uint.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    [HttpGet]
    public async Task<IActionResult> GetReviews()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var reviews = await _context.Reviews.Where(r => r.UserId == userId).Select(r => new
        {
            r.Id, r.UserId, UserName = r.User.Name, r.GroomerId,
            GroomerName = r.Groomer.Name, r.AppointmentId, r.Rating, r.Text, r.CreatedAt
        }).ToListAsync();
        return Ok(reviews);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReview(uint id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var review = await _context.Reviews.Where(r => r.Id == id && r.UserId == userId).Select(r => new
        {
            r.Id, r.UserId, UserName = r.User.Name, r.GroomerId,
            GroomerName = r.Groomer.Name, r.AppointmentId, r.Rating, r.Text, r.CreatedAt
        }).FirstOrDefaultAsync();
        return review == null ? NotFound() : Ok(review);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReview(Review review)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        if (review.Rating < 1 || review.Rating > 5)
            return BadRequest("Оценка должна быть от 1 до 5.");

        if (!await _context.Groomers.AnyAsync(g => g.Id == review.GroomerId))
            return BadRequest("Мастер не найден.");

        if (review.AppointmentId.HasValue)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a =>
                a.Id == review.AppointmentId.Value && a.UserId == userId && a.GroomerId == review.GroomerId);
            if (appointment == null)
                return BadRequest("Запись не найдена или не соответствует пользователю и мастеру.");
            if (await _context.Reviews.AnyAsync(r => r.AppointmentId == review.AppointmentId))
                return BadRequest("Для этой записи отзыв уже существует.");
        }

        review.Id = 0;
        review.UserId = userId;
        review.CreatedAt = DateTime.UtcNow;
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetReview), new { id = review.Id }, new { id = review.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReview(uint id, Review updatedReview)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
        if (review == null) return NotFound();
        if (updatedReview.Rating < 1 || updatedReview.Rating > 5)
            return BadRequest("Оценка должна быть от 1 до 5.");
        review.Rating = updatedReview.Rating;
        review.Text = updatedReview.Text;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Отзыв обновлён." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(uint id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
        if (review == null) return NotFound();
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
