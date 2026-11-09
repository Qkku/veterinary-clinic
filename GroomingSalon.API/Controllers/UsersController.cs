
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GroomingSalon.API.Data;
using GroomingSalon.API.Models;
using Microsoft.AspNetCore.Identity;
using GroomingSalon.API.DTOs;

namespace GroomingSalon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Получить всех пользователей
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.Email,
                u.Phone,
                u.IsAdmin,
                u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    // Получить пользователя по ID
    [HttpGet("{id}")]
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
                u.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    // Создать пользователя
    [HttpPost]
    public async Task<IActionResult> CreateUser(User user)
    {
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == user.Email);

        if (emailExists)
            return BadRequest("Пользователь с таким email уже существует.");

        user.Id = 0;
        user.CreatedAt = DateTime.UtcNow;
        user.IsAdmin = false;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetUser),
            new { id = user.Id },
            new { id = user.Id, user.Name, user.Email }
        );
    }

    // Изменить пользователя
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(uint id, User updatedUser)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == updatedUser.Email && u.Id != id);

        if (emailExists)
            return BadRequest("Этот email уже используется другим пользователем.");

        user.Name = updatedUser.Name;
        user.Email = updatedUser.Email;
        user.Phone = updatedUser.Phone;

        if (!string.IsNullOrWhiteSpace(updatedUser.PasswordHash))
            user.PasswordHash = updatedUser.PasswordHash;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Phone,
            user.IsAdmin,
            user.CreatedAt
        });
    }

    // Удалить пользователя
    [HttpDelete("{id}")]
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
