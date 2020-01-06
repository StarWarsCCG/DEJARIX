using System;
using System.Threading.Tasks;
using Dejarix.App.Entities;
using Microsoft.AspNetCore.SignalR;

namespace Dejarix.App.Hubs
{
    public class TradeHub : Hub<ITrader>
    {
        private readonly DejarixDbContext _dbContext;

        public TradeHub(DejarixDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        public Task<string> ReverseText(string text)
        {
            var reversed = string.Create(text.Length, text, (span, state) =>
            {
                int last = state.Length - 1;
                
                for (int i = 0; i < text.Length; ++i)
                    span[i] = state[last - i];
            });

            return Task.FromResult(reversed);
        }
    }
}