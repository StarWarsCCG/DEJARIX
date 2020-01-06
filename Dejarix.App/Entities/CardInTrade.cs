using System;

namespace Dejarix.App.Entities
{
    public class CardInTrade
    {
        public Guid TradeProposalId { get; set; }
        public Guid UserId { get; set; }
        public Guid CardId { get; set; }
        public string? Notes { get; set; }
        public int CardCount { get; set; }
    }
}