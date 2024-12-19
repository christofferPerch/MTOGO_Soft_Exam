namespace MTOGO.Web.Models.Order
{
    public class OrderManagementDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int? DeliveryAgentId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal VATAmount { get; set; }
        public DateTime OrderPlacedTimestamp { get; set; }
        public int OrderStatusId { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
