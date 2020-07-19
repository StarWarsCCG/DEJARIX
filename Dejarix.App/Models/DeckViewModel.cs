using Dejarix.App.Entities;

namespace Dejarix.App.Models
{
    public class DeckViewModel
    {
        public string PageTitle { get; set; } = string.Empty;
        public Deck Deck { get; set; }
        public bool ShowEditLink { get; set; }
    }
}