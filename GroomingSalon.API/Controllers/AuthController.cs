using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GroomingSalon.API.Data;
using GroomingSalon.API.DTOs;
using GroomingSalon.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GroomingSalon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthController(
        ApplicationDbContext context,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    // Вход по email и паролю. Возвращает JWT.
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserDto dto)
    {
        var email = dto.Email.Trim();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Одинаковый ответ для неизвестного email и неверного пароля.
        if (user == null)
            return Unauthorized(new { message = "Неверный email или пароль." });

        var verification = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, dto.Password);

        if (verification == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Неверный email или пароль." });

        // При необходимости обновляет хеш, если алгоритм/параметры хеширования изменились.
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            await _context.SaveChangesAsync();
        }

        var token = CreateToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(
            _configuration.GetValue<int?>("Jwt:ExpirationMinutes") ?? 60);

        return Ok(new
        {
            token,
            expiresAt,
            user = new
            {
                user.Id,
                user.Name,
                user.Email,
                user.Phone,
                user.IsAdmin
            }
        });
    }

    // Проверка токена и получение текущего пользователя.
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!uint.TryParse(idValue, out var userId))
            return Unauthorized();

        var user = await _context.Users
            .Where(u => u.Id == userId)
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
            return Unauthorized();

        return Ok(user);
    }

    private string CreateToken(User user)
    {
        var key = _configuration["Jwt:Key"]!;
        var issuer = _configuration["Jwt:Issuer"] ?? "GroomingSalon.API";
        var audience = _configuration["Jwt:Audience"] ?? "GroomingSalon.Frontend";
        var minutes = _configuration.GetValue<int?>("Jwt:ExpirationMinutes") ?? 60;
        if (minutes <= 0) minutes = 60;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.IsAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        else
            claims.Add(new Claim(ClaimTypes.Role, "Customer"));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
