using Microsoft.AspNetCore.SignalR;

namespace CommentsApp.API.Hubs
{
    public class CommentHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "comments");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "comments");
            await base.OnDisconnectedAsync(exception);
        }
    }
}