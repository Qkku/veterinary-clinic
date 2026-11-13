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
public class PetsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public PetsController(ApplicationDbContext context) => _context = context;

    private bool TryGetUserId(out uint userId) =>
        uint.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    [HttpGet]
    public async Task<IActionResult> GetPets()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var pets = await _context.Pets.Where(p => p.UserId == userId).Select(p => new
        {
            p.Id, p.UserId, p.Name, p.Age, p.Breed, p.Size, p.Notes, p.CreatedAt
        }).ToListAsync();
        return Ok(pets);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPet(uint id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var pet = await _context.Pets.Where(p => p.Id == id && p.UserId == userId).Select(p => new
        {
            p.Id, p.UserId, p.Name, p.Age, p.Breed, p.Size, p.Notes, p.CreatedAt
        }).FirstOrDefaultAsync();
        return pet == null ? NotFound() : Ok(pet);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePet(Pet pet)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        pet.Id = 0;
        pet.UserId = userId; // Владелец берётся из токена, а не из тела запроса.
        pet.CreatedAt = DateTime.UtcNow;
        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPet), new { id = pet.Id }, new
        {
            pet.Id, pet.UserId, pet.Name, pet.Age, pet.Breed, pet.Size, pet.Notes, pet.CreatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePet(uint id, Pet updatedPet)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (pet == null) return NotFound();
        pet.Name = updatedPet.Name;
        pet.Age = updatedPet.Age;
        pet.Breed = updatedPet.Breed;
        pet.Size = updatedPet.Size;
        pet.Notes = updatedPet.Notes;
        await _context.SaveChangesAsync();
        return Ok(new { pet.Id, pet.UserId, pet.Name, pet.Age, pet.Breed, pet.Size, pet.Notes, pet.CreatedAt });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePet(uint id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var pet = await _context.Pets.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (pet == null) return NotFound();
        _context.Pets.Remove(pet);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
