using CommentsApp.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using CommentsApp.API.Hubs;

public class RealTimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<CommentHub> _hub;

    public RealTimeNotifier(IHubContext<CommentHub> hub)
    {
        _hub = hub;
    }

    public async Task NewComment(object dto)
    {
        await _hub.Clients.Group("comments")
            .SendAsync("new_comment", dto);
    }
}