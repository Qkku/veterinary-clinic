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

    // Список пользователей доступен только администратору.
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users.Select(u => new
        {
            u.Id, u.Name, u.Email, u.Phone, u.IsAdmin, u.CreatedAt
        }).ToListAsync();
        return Ok(users);
    }

    // Получение произвольного пользователя — только администратору.
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUser(uint id)
    {
        var user = await _context.Users.Where(u => u.Id == id).Select(u => new
        {
            u.Id, u.Name, u.Email, u.Phone, u.IsAdmin, u.CreatedAt
        }).FirstOrDefaultAsync();
        return user == null ? NotFound() : Ok(user);
    }

    // Регистрация остаётся публичной.
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

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
        {
            user.Id, user.Name, user.Email, user.Phone, user.IsAdmin, user.CreatedAt
        });
    }

    // Администратор может редактировать пользователя. Пароль здесь не меняется.
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUser(uint id, User updatedUser)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        var email = updatedUser.Email.Trim();
        if (await _context.Users.AnyAsync(u => u.Email == email && u.Id != id))
            return Conflict(new { message = "Этот email уже используется другим пользователем." });

        user.Name = updatedUser.Name.Trim();
        user.Email = email;
        user.Phone = updatedUser.Phone?.Trim();
        await _context.SaveChangesAsync();

        return Ok(new { user.Id, user.Name, user.Email, user.Phone, user.IsAdmin, user.CreatedAt });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(uint id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
