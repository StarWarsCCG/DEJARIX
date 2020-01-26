using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Dejarix.App.Hubs
{
    public class LobbyHub : Hub<IPatron>
    {
        public LobbyHub()
        {
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}