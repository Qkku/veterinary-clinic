
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GroomingSalon.API.Data;
using GroomingSalon.API.Models;

namespace GroomingSalon.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PetsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PetsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Получить всех питомцев
    [HttpGet]
    public async Task<IActionResult> GetPets()
    {
        var pets = await _context.Pets
            .Select(p => new
            {
                p.Id,
                p.UserId,
                p.Name,
                p.Age,
                p.Breed,
                p.Size,
                p.Notes,
                p.CreatedAt
            })
            .ToListAsync();

        return Ok(pets);
    }

    // Получить питомца по ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPet(uint id)
    {
        var pet = await _context.Pets
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.UserId,
                p.Name,
                p.Age,
                p.Breed,
                p.Size,
                p.Notes,
                p.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (pet == null)
            return NotFound();

        return Ok(pet);
    }

    // Добавить питомца
    [HttpPost]
    public async Task<IActionResult> CreatePet(Pet pet)
    {
        pet.Id = 0;
        pet.CreatedAt = DateTime.UtcNow;

        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetPet),
            new { id = pet.Id },
            pet
        );
    }

    // Изменить данные питомца
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePet(uint id, Pet updatedPet)
    {
        var pet = await _context.Pets.FindAsync(id);

        if (pet == null)
            return NotFound();

        pet.Name = updatedPet.Name;
        pet.Age = updatedPet.Age;
        pet.Breed = updatedPet.Breed;
        pet.Size = updatedPet.Size;
        pet.Notes = updatedPet.Notes;

        await _context.SaveChangesAsync();

        return Ok(pet);
    }

    // Удалить питомца
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePet(uint id)
    {
        var pet = await _context.Pets.FindAsync(id);

        if (pet == null)
            return NotFound();

        _context.Pets.Remove(pet);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}