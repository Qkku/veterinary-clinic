namespace GroomingSalon.API.Models;

public class LeaveRequest
{
    public uint Id { get; set; }
    public uint GroomerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = null!;
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
    public Groomer Groomer { get; set; } = null!;
}
