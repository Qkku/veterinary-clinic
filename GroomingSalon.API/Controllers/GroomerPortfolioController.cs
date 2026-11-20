using GroomingSalon.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroomingSalon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroomerPortfolioController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public GroomerPortfolioController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet("{groomerId:int}")]
    public async Task<IActionResult> GetPortfolio(uint groomerId)
    {
        var exists = await _context.Groomers.AnyAsync(g => g.Id == groomerId);
        if (!exists) return NotFound(new { message = "Груммер не найден." });

        var photos = await _context.GroomerPortfolios
            .Where(p => p.GroomerId == groomerId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new { p.Id, p.GroomerId, p.ImageUrl, p.CreatedAt })
            .ToListAsync();

        return Ok(photos);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePhoto(uint id)
    {
        var photo = await _context.GroomerPortfolios.FindAsync(id);
        if (photo == null) return NotFound();

        var imageUrl = photo.ImageUrl;
        _context.GroomerPortfolios.Remove(photo);
        await _context.SaveChangesAsync();

        const string prefix = "/uploads/portfolio/";
        if (imageUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            var fileName = Path.GetFileName(imageUrl);
            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
                webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");

            var path = Path.Combine(webRoot, "uploads", "portfolio", fileName);
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        return NoContent();
    }
}
