using System;

namespace Dejarix.App.Entities
{
    public class CardInDeckRevision
    {
        public Guid DeckRevisionId { get; set; }
        public Guid CardId { get; set; }
        public int InsideCount { get; set; }
        public int OutsideCount { get; set; }

        public DeckRevision? DeckRevision { get; set; }
        public CardImage? Card { get; set; }
    }
}