namespace MTOGO.Web.Models.ShoppingCart
{
    public class CartRequestMessageDto
    {
        public string UserId { get; set; }
        public Guid CorrelationId { get; set; }
    }
}
