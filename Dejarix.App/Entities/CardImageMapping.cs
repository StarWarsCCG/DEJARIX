using System;
using System.ComponentModel.DataAnnotations;

namespace Dejarix.App.Entities
{
    public class CardImageMapping
    {
        public const string Gemp = "gemp";
        public const string Holotable = "holotable";

        [Required] public string Group { get; set; } = string.Empty;
        [Required] public string ExternalId { get; set; } = string.Empty;
        public Guid CardImageId { get; set; }
    }
}