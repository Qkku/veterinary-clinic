using GroomingSalon.API.Data;
using GroomingSalon.API.DTOs;
using GroomingSalon.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroomingSalon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UsersController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users.Select(u => new
        {
            u.Id,
            u.Name,
            u.Email,
            u.Phone,
            u.IsAdmin,
            u.IsBanned,
            u.ImageUrl,
            u.CreatedAt
        }).ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUser(uint id)
    {
        var user = await _context.Users
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.Phone,
                u.IsAdmin,
                u.IsBanned,
                u.ImageUrl,
                u.CreatedAt
            })
            .FirstOrDefaultAsync();

        return user == null ? NotFound() : Ok(user);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterUserDto dto)
    {
        var email = dto.Email.Trim();

        if (await _context.Users.AnyAsync(u => u.Email == email))
            return Conflict(new { message = "Пользователь с таким email уже существует" });

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = email,
            Phone = dto.Phone?.Trim(),
            IsAdmin = false,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return StatusCode(StatusCodes.Status201Created, new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Phone,
            user.IsAdmin,
            user.ImageUrl,
            user.CreatedAt
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(uint id, User updatedUser)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        var email = updatedUser.Email.Trim();
        if (await _context.Users.AnyAsync(u => u.Email == email && u.Id != id))
            return Conflict(new { message = "Этот email уже используется другим пользователем." });

        user.Name = updatedUser.Name.Trim();
        user.Email = email;
        user.Phone = updatedUser.Phone?.Trim();
        // ImageUrl меняется через ImagesController.

        await _context.SaveChangesAsync();

        return Ok(new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Phone,
            user.IsAdmin,
            user.IsBanned,
            user.ImageUrl,
            user.CreatedAt
        });
    }

    [HttpPut("{id:int}/ban")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BanUser(uint id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound(new { message = "Пользователь не найден." });

        if (user.IsAdmin)
            return BadRequest(new { message = "Нельзя заблокировать администратора." });

        user.IsBanned = true;

        var futureAppointments = await _context.Appointments
            .Where(a => a.Pet.UserId == id
                && a.StartDatetime > DateTime.Now
                && (a.Status == "booked" || a.Status == "confirmed"))
            .ToListAsync();

        foreach (var appointment in futureAppointments)
            appointment.Status = "cancelled";

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Пользователь заблокирован.",
            userId = user.Id,
            user.IsBanned,
            cancelledAppointments = futureAppointments.Count
        });
    }

    [HttpPut("{id:int}/unban")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UnbanUser(uint id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound(new { message = "Пользователь не найден." });

        user.IsBanned = false;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Пользователь разблокирован.",
            userId = user.Id,
            user.IsBanned
        });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(uint id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
