using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dejarix.App.Entities
{
    public class DeckRevision
    {
        [Key] public Guid DeckRevisionId { get; set; }
        public Guid? ParentId { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public Guid CreatorId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }

        public DejarixUser? Creator { get; set; }
        public DeckRevision? Parent { get; set; }
        public List<CardInDeckRevision>? Cards { get; set; }
    }
}