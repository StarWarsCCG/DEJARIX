using System;

namespace Dejarix.App.Entities
{
    public class CardInventory
    {
        public Guid UserId { get; set; }
        public DejarixUser? User { get; set; }
        public Guid CardImageId { get; set; }
        public CardImage? CardImage { get; set; }
        public string? PublicNotes { get; set; }
        public int PublicHaveCount { get; set; }
        public int PublicWantCount { get; set; }
        public string? PrivateNotes { get; set; }
        public int PrivateHaveCount { get; set; }
        public int PrivateWantCount { get; set; }
    }
}