namespace MTOGO.Web.Models.ShoppingCart
{
    public class CartResponseMessageDto
    {
        public string UserId { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public Guid CorrelationId { get; set; }
    }
}
