namespace MTOGO.Services.OrderAPI.Models.Dto
{
    public class CartResponseMessageDto
    {
        public string UserId { get; set; }
        public string CustomerEmail { get; set; }
        public Guid CorrelationId { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }
    }
}
