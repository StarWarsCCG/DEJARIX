using System;
using System.ComponentModel.DataAnnotations;

namespace Dejarix.App.Entities
{
    public class Game
    {
        [Key] public Guid GameId { get; set; }
        public Guid CreatorId { get; set; }
        public Guid? InviteeId { get; set; }
        public Guid? DarkPlayerId { get; set; }
        public Guid? LightPlayerId { get; set; }
        public Guid? DarkDeckRevisionId { get; set; }
        public Guid? LightDeckRevisionId { get; set; }
        public string GameName { get; set; } // Title to show in the lobby.
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset? DateStarted { get; set; }
        public DateTimeOffset? DateFinished { get; set; }
        public bool AllowSpectators { get; set; }
        public bool AllowSpectatorChat { get; set; }
        public bool PublicPlayerHands { get; set; } // Allow spectators to see players' hands.
        public bool MixedChat { get; set; } // Allow players to see spectator chat.

        public DejarixUser Creator { get; set; }
    }
}