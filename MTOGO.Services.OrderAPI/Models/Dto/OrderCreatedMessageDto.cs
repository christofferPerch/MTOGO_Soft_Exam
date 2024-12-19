namespace MTOGO.Services.OrderAPI.Models.Dto
{
    public class OrderCreatedMessageDto
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public string CustomerEmail { get; set; } // New field
        public decimal TotalAmount { get; set; }

        public decimal VATAmount { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }
}
