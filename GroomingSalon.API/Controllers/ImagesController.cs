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
public class ImagesController : ControllerBase
{
    private const long MaxFileSize = 5 * 1024 * 1024;

    private readonly IWebHostEnvironment _environment;
    private readonly ApplicationDbContext _context;

    public ImagesController(IWebHostEnvironment environment, ApplicationDbContext context)
    {
        _environment = environment;
        _context = context;
    }

    [HttpPost("{category}/{id:int}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> Upload(string category, uint id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Файл не выбран." });

        if (file.Length > MaxFileSize)
            return BadRequest(new { message = "Размер файла не должен превышать 5 МБ." });

        var role = User.FindFirstValue(ClaimTypes.Role);
        if (!uint.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var currentUserId))
            return Unauthorized();

        category = category.ToLowerInvariant();
        string? oldImageUrl = null;

        switch (category)
        {
            case "users":
            {
                if (role != "Admin" && (role != "Customer" || id != currentUserId))
                    return Forbid();

                var user = await _context.Users.FindAsync(id);
                if (user == null) return NotFound(new { message = "Пользователь не найден." });
                oldImageUrl = user.ImageUrl;
                break;
            }
            case "pets":
            {
                var pet = await _context.Pets.FindAsync(id);
                if (pet == null) return NotFound(new { message = "Питомец не найден." });
                if (role != "Admin" && (role != "Customer" || pet.UserId != currentUserId))
                    return Forbid();
                oldImageUrl = pet.ImageUrl;
                break;
            }
            case "groomers":
            {
                if (role != "Admin" && (role != "Groomer" || id != currentUserId))
                    return Forbid();
                var groomer = await _context.Groomers.FindAsync(id);
                if (groomer == null) return NotFound(new { message = "Груммер не найден." });
                oldImageUrl = groomer.ImageUrl;
                break;
            }
            case "services":
            case "portfolio":
            {
                if (role != "Admin") return Forbid();
                if (category == "services")
                {
                    if (!await _context.Services.AnyAsync(s => s.Id == id))
                        return NotFound(new { message = "Услуга не найдена." });
                }
                else if (!await _context.Groomers.AnyAsync(g => g.Id == id))
                {
                    return NotFound(new { message = "Груммер не найден." });
                }
                break;
            }
            default:
                return BadRequest(new { message = "Неизвестная категория изображения." });
        }

        var header = new byte[12];
        await using var input = file.OpenReadStream();
        var bytesRead = await input.ReadAsync(header);
        input.Position = 0;

        var extension = GetImageExtension(header, bytesRead);
        if (extension == null)
            return BadRequest(new { message = "Допустимые форматы: JPG, PNG, WEBP." });

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");

        var folder = Path.Combine(webRoot, "uploads", category);
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(folder, fileName);
        var imageUrl = $"/uploads/{category}/{fileName}";

        await using (var output = new FileStream(filePath, FileMode.CreateNew))
        {
            await input.CopyToAsync(output);
        }

        try
        {
            switch (category)
            {
                case "users":
                    (await _context.Users.FindAsync(id))!.ImageUrl = imageUrl;
                    break;
                case "pets":
                    (await _context.Pets.FindAsync(id))!.ImageUrl = imageUrl;
                    break;
                case "groomers":
                    (await _context.Groomers.FindAsync(id))!.ImageUrl = imageUrl;
                    break;
                case "services":
                    (await _context.Services.FindAsync(id))!.ImageUrl = imageUrl;
                    break;
                case "portfolio":
                    _context.GroomerPortfolios.Add(new GroomerPortfolio
                    {
                        GroomerId = id,
                        ImageUrl = imageUrl,
                        CreatedAt = DateTime.UtcNow
                    });
                    break;
            }

            await _context.SaveChangesAsync();
        }
        catch
        {
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
            throw;
        }


        if (!string.IsNullOrWhiteSpace(oldImageUrl))
            DeleteLocalImage(oldImageUrl, category);

        return Ok(new { imageUrl });
    }

    private void DeleteLocalImage(string imageUrl, string category)
    {
        var expectedPrefix = $"/uploads/{category}/";
        if (!imageUrl.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            return;

        var fileName = Path.GetFileName(imageUrl);
        if (string.IsNullOrWhiteSpace(fileName)) return;

        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");

        var path = Path.Combine(webRoot, "uploads", category, fileName);
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
    }

    private static string? GetImageExtension(byte[] buffer, int length)
    {
        if (length >= 3 && buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF)
            return ".jpg";

        if (length >= 8 && buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E &&
            buffer[3] == 0x47 && buffer[4] == 0x0D && buffer[5] == 0x0A &&
            buffer[6] == 0x1A && buffer[7] == 0x0A)
            return ".png";

        if (length >= 12 && buffer[0] == 'R' && buffer[1] == 'I' && buffer[2] == 'F' &&
            buffer[3] == 'F' && buffer[8] == 'W' && buffer[9] == 'E' &&
            buffer[10] == 'B' && buffer[11] == 'P')
            return ".webp";

        return null;
    }
}
