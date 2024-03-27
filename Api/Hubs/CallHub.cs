using Microsoft.AspNetCore.SignalR;

namespace Api.Hubs
{
    public class CallHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
