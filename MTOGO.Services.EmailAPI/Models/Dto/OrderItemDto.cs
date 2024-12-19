namespace MTOGO.Services.EmailAPI.Models.Dto
{

    public class OrderItemDto
    {
        public string Name { get; set; } = string.Empty;
        public int RestaurantId { get; set; }
        public string MenuItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
