namespace MTOGO.Web.Models.Order
{
    public class OrderDto
    {
        public string UserId { get; set; }
        public int? DeliveryAgentId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal VATAmount { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }
}
