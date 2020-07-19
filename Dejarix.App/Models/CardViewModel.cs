using Dejarix.App.Entities;

namespace Dejarix.App.Models
{
    public class CardViewModel
    {
        public CardInventory Inventory { get; set; }
        public string Title { get; set; }
        public string FrontImage { get; set; }
        public string BackImage { get; set; }
        public bool IsHorizontal { get; set; }
    }
}