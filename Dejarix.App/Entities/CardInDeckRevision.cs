using System;

namespace Dejarix.App.Entities
{
    public class CardInDeckRevision
    {
        public Guid CardCollectionId { get; set; }
        public DeckRevision? CardCollection { get; set; }
        public Guid CardId { get; set; }
        public CardImage? Card { get; set; }
        public int StartCount { get; set; }
        public int CardCount { get; set; }
        public int OutsideCount { get; set; }
    }
}