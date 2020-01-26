using System;
using System.Threading.Tasks;

namespace Dejarix.App.Hubs
{
    public interface IPatron
    {
        Task UserEntered(Guid userId);
        Task UserLeft(Guid userId);
        Task PublicChat(Guid senderId, string message);
        Task PrivateChat(Guid senderId, string message);
        Task InviteToPlay(Guid inviterId, Guid gameId);
        Task InviteToSpectate(Guid inviterId, Guid gameId);
    }
}