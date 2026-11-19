
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroomingSalon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ImagesController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    private const long MaxFileSize = 5 * 1024 * 1024; // 5 МБ

    private static readonly Dictionary<string, string> AllowedFolders = new()
    {
        ["pets"] = "pets",
        ["groomers"] = "groomers",
        ["users"] = "users",
        ["services"] = "services",
        ["portfolio"] = "portfolio"
    };

    public ImagesController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost("{category}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> Upload(
        string category,
        IFormFile file)
    {
        if (!AllowedFolders.TryGetValue(category.ToLower(), out var folder))
            return BadRequest("Неизвестная категория изображения.");

        if (file == null || file.Length == 0)
            return BadRequest("Файл не выбран.");

        if (file.Length > MaxFileSize)
            return BadRequest("Размер файла не должен превышать 5 МБ.");


        var buffer = new byte[12];

        await using var input = file.OpenReadStream();
        var bytesRead = await input.ReadAsync(buffer);
        input.Position = 0;

        var extension = GetImageExtension(buffer, bytesRead);

        if (extension == null)
            return BadRequest("Допустимые форматы: JPG, PNG, WEBP.");

        var fileName = $"{Guid.NewGuid()}{extension}";

        var uploadsPath = Path.Combine(
            _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"),
            "uploads",
            folder
        );

        Directory.CreateDirectory(uploadsPath);

        var filePath = Path.Combine(uploadsPath, fileName);

        await using (var output = new FileStream(filePath, FileMode.Create))
        {
            await input.CopyToAsync(output);
        }

        var imageUrl = $"/uploads/{folder}/{fileName}";

        return Ok(new { imageUrl });
    }

    private static string? GetImageExtension(byte[] buffer, int length)
    {
        // JPEG
        if (length >= 3 &&
            buffer[0] == 0xFF &&
            buffer[1] == 0xD8 &&
            buffer[2] == 0xFF)
            return ".jpg";

        // PNG
        if (length >= 8 &&
            buffer[0] == 0x89 &&
            buffer[1] == 0x50 &&
            buffer[2] == 0x4E &&
            buffer[3] == 0x47 &&
            buffer[4] == 0x0D &&
            buffer[5] == 0x0A &&
            buffer[6] == 0x1A &&
            buffer[7] == 0x0A)
            return ".png";

        // WEBP
        if (length >= 12 &&
            buffer[0] == 'R' &&
            buffer[1] == 'I' &&
            buffer[2] == 'F' &&
            buffer[3] == 'F' &&
            buffer[8] == 'W' &&
            buffer[9] == 'E' &&
            buffer[10] == 'B' &&
            buffer[11] == 'P')
            return ".webp";

        return null;
    }
}
