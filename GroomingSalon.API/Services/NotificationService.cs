
using GroomingSalon.API.Data;
using GroomingSalon.API.Hubs;
using GroomingSalon.API.Models;
using Microsoft.AspNetCore.SignalR;

namespace GroomingSalon.API.Services;

public class NotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hub;

    public NotificationService(
        ApplicationDbContext context,
        IHubContext<NotificationHub> hub)
    {
        _context = context;
        _hub = hub;
    }

    public async Task NotifyGroomerAsync(
        uint groomerId,
        string title,
        string message,
        string type)
    {
        var notification = new Notification
        {
            GroomerId = groomerId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        await _hub.Clients
            .Group($"groomer-{groomerId}")
            .SendAsync("ReceiveNotification", new
            {
                notification.Id,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.IsRead,
                notification.CreatedAt
            });
    }

    public async Task NotifyUserAsync(
        uint userId,
        string title,
        string message,
        string type)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        await _hub.Clients
            .Group($"user-{userId}")
            .SendAsync("ReceiveNotification", new
            {
                notification.Id,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.IsRead,
                notification.CreatedAt
            });
    }
}
