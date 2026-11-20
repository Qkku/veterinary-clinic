namespace GroomingSalon.API.Models;

public class GroomerPortfolio
{
    public uint Id { get; set; }
    public uint GroomerId { get; set; }
    public string ImageUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
