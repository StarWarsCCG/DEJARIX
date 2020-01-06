using System;

namespace Dejarix.App.Entities
{
    public class TradeProposal
    {
        public Guid TradeProposalId { get; set; }
        public Guid TradeId { get; set; }
        public Guid UserId { get; set; } // User who created this proposal.
        public Guid? PreviousProposalId { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}