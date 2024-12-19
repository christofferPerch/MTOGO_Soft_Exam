namespace MTOGO.Services.EmailAPI.Models.Dto 
{

    public class OrderCreatedMessageDto {
        public int OrderId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
        public decimal TotalAmount { get; set; }
    }
}
