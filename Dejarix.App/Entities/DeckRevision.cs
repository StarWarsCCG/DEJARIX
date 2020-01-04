using System;
using System.ComponentModel.DataAnnotations;

namespace Dejarix.App.Entities
{
    public class DeckRevision
    {
        [Key] public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public DeckRevision? Parent { get; set; }
        public DateTimeOffset CreationDate { get; set; }
        public Guid CreatorId { get; set; }
        public DejarixUser? Creator { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}