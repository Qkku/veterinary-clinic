
using System.Security.Claims;
using GroomingSalon.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroomingSalon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NotificationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    private bool TryGetRecipient(out uint id, out bool isGroomer)
    {
        isGroomer =
            User.FindFirstValue(ClaimTypes.Role) == "Groomer";

        return uint.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out id);
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        if (!TryGetRecipient(out var id, out var isGroomer))
            return Unauthorized();

        var query = _context.Notifications.AsQueryable();

        query = isGroomer
            ? query.Where(n => n.GroomerId == id)
            : query.Where(n => n.UserId == id);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(100)
            .Select(n => new
            {
                n.Id,
                n.Title,
                n.Message,
                n.Type,
                n.IsRead,
                n.CreatedAt
            })
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(uint id)
    {
        if (!TryGetRecipient(out var recipientId, out var isGroomer))
            return Unauthorized();

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n =>
                n.Id == id &&
                (isGroomer
                    ? n.GroomerId == recipientId
                    : n.UserId == recipientId));

        if (notification == null)
            return NotFound();

        notification.IsRead = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Уведомление прочитано." });
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        if (!TryGetRecipient(out var recipientId, out var isGroomer))
            return Unauthorized();

        var notifications = await _context.Notifications
            .Where(n =>
                (isGroomer
                    ? n.GroomerId == recipientId
                    : n.UserId == recipientId) &&
                !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
            notification.IsRead = true;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Уведомления прочитаны.",
            count = notifications.Count
        });
    }
}
