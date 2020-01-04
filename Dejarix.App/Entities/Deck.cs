using System;
using System.ComponentModel.DataAnnotations;

namespace Dejarix.App.Entities
{
    public class Deck
    {
        [Key] public Guid Id { get; set; }
        public bool IsPublic { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public DateTimeOffset? DeletionDate { get; set; }
        public Guid CreatorId { get; set; }
        public DejarixUser? Creator { get; set; }
        public Guid RevisionId { get; set; }
        public DeckRevision? Revision { get; set; }
    }
}