
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GroomingSalon.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var idValue = Context.User?
            .FindFirstValue(ClaimTypes.NameIdentifier);

        var role = Context.User?
            .FindFirstValue(ClaimTypes.Role);

        if (uint.TryParse(idValue, out var id))
        {
            string group = role == "Groomer"
                ? $"groomer-{id}"
                : $"user-{id}";

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                group);
        }

        await base.OnConnectedAsync();
    }
}
