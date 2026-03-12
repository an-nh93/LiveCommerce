using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LiveCommerce.Api.Hubs;

[Authorize]
public class CommentHub : Hub
{
    public async Task JoinShop(long shopId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"shop_{shopId}");
    }

    public async Task JoinLiveSession(long liveSessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"live_{liveSessionId}");
    }

    public async Task LeaveLiveSession(long liveSessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"live_{liveSessionId}");
    }
}
