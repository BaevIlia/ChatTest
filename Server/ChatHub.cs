using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Server
{
    [Authorize]
    public class ChatHub : Hub
    {
        
        public async Task Send(string username, string message) 
        {
              
            await this.Clients.All.SendAsync("Receive", username, message);
        }
    }
}
