using System;
using System.ComponentModel.DataAnnotations;

namespace Dejarix.App.Entities
{
    public class TradeMessage
    {
        [Key] public Guid MessageId { get; set; }
        public Guid TradeId { get; set; }
        public Guid UserId { get; set; }
        public DateTimeOffset TimeSent { get; set; }
        [Required] public string MessageContent { get; set; } = string.Empty;
    }
}