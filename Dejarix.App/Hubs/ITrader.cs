using System;
using System.Threading.Tasks;

namespace Dejarix.App.Hubs
{
    public interface ITrader
    {
        Task MessageSent(Guid userId, string messageContent);
        Task ProposeTrade(Guid userId);
    }
}