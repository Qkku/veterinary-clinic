
namespace GroomingSalon.API.Models;

public class Notification
{
    public uint Id { get; set; }

    public uint? UserId { get; set; }

    public uint? GroomerId { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string Type { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}
