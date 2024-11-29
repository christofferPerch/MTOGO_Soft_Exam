namespace MTOGO.Web.Models.Order
{
    public class CheckoutDto
    {
        public int RestaurantId { get; set; }
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Name { get; set; } // Name of the menu item
        public byte[] ?Image { get; set; } // Image URL
    }
}
