using System;

namespace Dejarix.App.Entities
{
    public class CardInDeckRevision
    {
        public Guid DeckRevisionId { get; set; }
        public DeckRevision? CardCollection { get; set; }
        public Guid CardId { get; set; }
        public CardImage? Card { get; set; }
        public int Ordinal { get; set; }
        public int InsideCount { get; set; }
        public int OutsideCount { get; set; }
    }
}