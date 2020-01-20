using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;

namespace Dejarix.App.Entities
{
    public class CardImage
    {
        [Key] public Guid Id { get; set; }
        public Guid OtherId { get; set; }
        public bool IsLightSide { get; set; }
        public bool IsFront { get; set; }
        public bool IsHorizontal { get; set; }
        [Required] public string Title { get; set; } = string.Empty;
        [Required] public string TitleNormalized { get; set; } = string.Empty;
        [Required] public string Destiny { get; set; } = string.Empty;
        [Required] public string Expansion { get; set; } = string.Empty;
        [Required] public string InfoJson { get; set; } = string.Empty;

        public static CardImage FromJson(JsonElement json)
        {
            var result = new CardImage
            {
                Id = Guid.Parse(json.GetProperty("ImageId").GetString()),
                OtherId = Guid.Parse(json.GetProperty("OtherImageId").GetString()),
                IsLightSide = json.GetProperty("IsLightSide").GetBoolean(),
                IsFront = json.GetProperty("IsFront").GetBoolean(),
                IsHorizontal = json
                    .GetProperty("SecondaryTypes")
                    .EnumerateArray()
                    .Any(je => je.GetString() == "Site"),
                Title = json.GetProperty("CardName").GetString(),
                Destiny = json.GetProperty("Destiny").GetString(),
                Expansion = json.GetProperty("Expansion").GetString(),
                InfoJson = json.ToString()
            };

            result.TitleNormalized = result.Title.NormalizedForSearch();

            return result;
        }
    }
}