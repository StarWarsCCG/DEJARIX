using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Dejarix.App.Hubs
{
    public class PlayerHub : Hub<IPlayer>
    {
        public PlayerHub()
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