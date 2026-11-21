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
public class LeaveRequestsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public LeaveRequestsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll() => Ok(await _db.LeaveRequests.Include(x => x.Groomer).OrderByDescending(x => x.CreatedAt)
        .Select(x => new { x.Id, x.GroomerId, GroomerName = x.Groomer.Name, x.StartDate, x.EndDate, x.Reason, x.Status, x.CreatedAt }).ToListAsync());

    [HttpGet("my")]
    [Authorize(Roles = "Groomer")]
    public async Task<IActionResult> GetMine()
    {
        if (!uint.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return Unauthorized();
        return Ok(await _db.LeaveRequests.Where(x => x.GroomerId == id).OrderByDescending(x => x.CreatedAt)
            .Select(x => new { x.Id, x.GroomerId, x.StartDate, x.EndDate, x.Reason, x.Status, x.CreatedAt }).ToListAsync());
    }

    [HttpPost]
    [Authorize(Roles = "Groomer")]
    public async Task<IActionResult> Create(LeaveCreateDto dto)
    {
        if (!uint.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return Unauthorized();
        var minLeaveDate = DateTime.Today.AddMonths(3);

        if (string.IsNullOrWhiteSpace(dto.Reason) ||
            dto.Reason.Trim().Length > 500 ||
            dto.StartDate.Date < minLeaveDate ||
            dto.EndDate.Date < dto.StartDate.Date)
        {
            return BadRequest(new
            {
                message = "Отпуск можно запрашивать не ранее чем за три месяца до его начала. Проверьте даты."
            });
        }
        var entity = new LeaveRequest { GroomerId = id, StartDate = dto.StartDate.Date, EndDate = dto.EndDate.Date, Reason = dto.Reason.Trim(), Status = "pending", CreatedAt = DateTime.UtcNow };
        _db.LeaveRequests.Add(entity); await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMine), new { id = entity.Id }, new { entity.Id, entity.Status });
    }

    [HttpPatch("{id:int}/decision")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Decide(uint id, LeaveDecisionDto dto)
    {
        if (dto.Status is not ("approved" or "rejected")) return BadRequest(new { message = "Допустимые статусы: approved, rejected." });
        var request = await _db.LeaveRequests.Include(x => x.Groomer).FirstOrDefaultAsync(x => x.Id == id);
        if (request == null) return NotFound();
        if (request.Status != "pending") return Conflict(new { message = "Заявка уже обработана." });
        if (dto.Status == "approved" && request.StartDate.Date < DateTime.Today.AddMonths(3))
        {
            return Conflict(new
            {
                message = "Нельзя одобрить заявку: отпуск должен начинаться не раньше чем через три месяца."
            });
        }
        request.Status = dto.Status;
        if (dto.Status == "approved") _db.GroomerTimeOffs.Add(new GroomerTimeOff { GroomerId = request.GroomerId, StartDatetime = request.StartDate.Date, EndDatetime = request.EndDate.Date.AddDays(1).AddTicks(-1), Type = "vacation", Comment = request.Reason });
        await _db.SaveChangesAsync();
        return Ok(new { request.Id, request.Status });
    }
}
public class LeaveCreateDto { public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } public string Reason { get; set; } = ""; }
public class LeaveDecisionDto { public string Status { get; set; } = ""; }
