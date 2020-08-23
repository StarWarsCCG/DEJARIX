using System;

namespace Dejarix.App.Entities
{
    public class DiscussionThread
    {
        public Guid CreatorId { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Deleted { get; set; }
        public string Subject { get; set; }
    }
}