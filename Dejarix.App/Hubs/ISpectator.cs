using System;
using System.Threading.Tasks;

namespace Dejarix.App.Hubs
{
    public interface ISpectator
    {
        Task DoTheThing();
        Task SpectatorChat(Guid senderId, string message);
        Task PlayerChat(Guid senderId, string message);
    }
}