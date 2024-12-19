namespace MTOGO.Services.OrderAPI.Models.Dto
{
    public class AddOrderDto
    {
        public string UserId { get; set; }
        public string CustomerEmail { get; set; } // New field
        public decimal TotalAmount { get; set; }
        public decimal VATAmount { get; set; }
        public List<CartItem> Items { get; set; }
    }


}
