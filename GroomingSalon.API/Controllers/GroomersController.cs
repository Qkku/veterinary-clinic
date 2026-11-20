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
public class GroomersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<Groomer> _passwordHasher;

    public GroomersController(
        ApplicationDbContext context,
        IPasswordHasher<Groomer> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public async Task<IActionResult> GetGroomers()
    {
        var groomers = await _context.Groomers
            .Select(g => new
            {
                g.Id,
                g.Name,
                g.Email,
                g.Phone,
                g.Age,
                g.Education,
                g.ExperienceYears,
                g.Specialization,
                g.IsActive,
                g.ImageUrl,
                g.CreatedAt
            })
            .ToListAsync();

        return Ok(groomers);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetGroomer(uint id)
    {
        var groomer = await _context.Groomers
            .Where(g => g.Id == id)
            .Select(g => new
            {
                g.Id,
                g.Name,
                g.Email,
                g.Phone,
                g.Age,
                g.Education,
                g.ExperienceYears,
                g.Specialization,
                g.IsActive,
                g.ImageUrl,
                g.CreatedAt
            })
            .FirstOrDefaultAsync();

        return groomer == null ? NotFound() : Ok(groomer);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateGroomer(CreateGroomerDto dto)
    {
        var email = dto.Email.Trim();

        if (await _context.Groomers.AnyAsync(g => g.Email == email))
            return Conflict(new { message = "Мастер с таким email уже существует." });

        var groomer = new Groomer
        {
            Name = dto.Name.Trim(),
            Email = email,
            Phone = dto.Phone,
            Age = dto.Age,
            Education = dto.Education,
            ExperienceYears = dto.ExperienceYears,
            Specialization = dto.Specialization,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        groomer.PasswordHash = _passwordHasher.HashPassword(groomer, dto.Password);

        _context.Groomers.Add(groomer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGroomer), new { id = groomer.Id }, new
        {
            groomer.Id,
            groomer.Name,
            groomer.Email,
            groomer.ImageUrl
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateGroomer(uint id, UpdateGroomerDto dto)
    {
        var groomer = await _context.Groomers.FindAsync(id);

        if (groomer == null)
            return NotFound();

        var email = dto.Email.Trim();
        var emailExists = await _context.Groomers
            .AnyAsync(g => g.Email == email && g.Id != id);

        if (emailExists)
            return Conflict(new { message = "Этот email уже используется другим мастером." });

        groomer.Name = dto.Name.Trim();
        groomer.Email = email;
        groomer.Phone = dto.Phone;
        groomer.Age = dto.Age;
        groomer.Education = dto.Education;
        groomer.ExperienceYears = dto.ExperienceYears;
        groomer.Specialization = dto.Specialization;
        groomer.IsActive = dto.IsActive;
        // ImageUrl изменяется через ImagesController, чтобы не затереть его пустым значением DTO.

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            if (dto.Password.Length < 8)
                return BadRequest(new { message = "Пароль должен содержать минимум 8 символов." });

            groomer.PasswordHash = _passwordHasher.HashPassword(groomer, dto.Password);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            groomer.Id,
            groomer.Name,
            groomer.Email,
            groomer.Phone,
            groomer.Age,
            groomer.Education,
            groomer.ExperienceYears,
            groomer.Specialization,
            groomer.IsActive,
            groomer.ImageUrl,
            groomer.CreatedAt
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteGroomer(uint id)
    {
        var groomer = await _context.Groomers.FindAsync(id);

        if (groomer == null)
            return NotFound();

        _context.Groomers.Remove(groomer);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
