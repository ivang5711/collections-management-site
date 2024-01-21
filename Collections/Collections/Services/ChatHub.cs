﻿using Microsoft.AspNetCore.SignalR;

namespace Collections.Services
{
    public class ChatHub : Hub
    {
        public  Task SendMessage(string user, string message)
        =>  Clients.All.SendAsync("ReceiveMessage", user, message);
    }
}
