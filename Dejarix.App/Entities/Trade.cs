using System;
using System.ComponentModel.DataAnnotations;

namespace Dejarix.App.Entities
{
    public class Trade
    {
        [Key] public Guid TradeId { get; set; }
        public Guid FirstUserId { get; set; } // User who initiated the trade.
        public Guid SecondUserId { get; set; }
        public Guid CurrentProposalId { get; set; }
        public DateTimeOffset Started { get; set; }
        public DateTimeOffset? Completed { get; set; }
        public string? Resolution { get; set; }
    }
}