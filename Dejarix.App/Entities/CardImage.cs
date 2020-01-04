using System;
using System.ComponentModel.DataAnnotations;

namespace Dejarix.App.Entities
{
    public class CardImage
    {
        [Key] public Guid Id { get; set; }
        public Guid OtherId { get; set; }
        public bool IsLightSide { get; set; }
        public bool IsFront { get; set; }
        [Required] public string Title { get; set; } = string.Empty;
        [Required] public string TitleNormalized { get; set; } = string.Empty;
        [Required] public string Destiny { get; set; } = string.Empty;
        [Required] public string Expansion { get; set; } = string.Empty;
        [Required] public string InfoJson { get; set; } = string.Empty;
    }
}