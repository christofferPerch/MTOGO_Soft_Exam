using MTOGO.Services.ShoppingCartAPI.Models.Dto;

namespace MTOGO.Services.ShoppingCartAPI.Models.Events
{
    public class CartCreatedOrUpdatedEvent
    {
        public string UserId { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
        public decimal TotalAmount { get; set; }  
        public Guid CorrelationId { get; set; }  
        public DateTime Timestamp { get; set; }
    }
}
