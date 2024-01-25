using Microsoft.AspNetCore.SignalR;

namespace Collections.Services;

public class CommentsHub : Hub
{
    public Task UpdateComments() => Clients.All.SendAsync("ReceiveUpdateComments");
}