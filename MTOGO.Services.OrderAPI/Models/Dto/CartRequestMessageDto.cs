namespace MTOGO.Services.OrderAPI.Models.Dto
{
    public class CartRequestMessageDto
    {
        public string UserId { get; set; }
        public string CustomerEmail { get; set; }
        public Guid CorrelationId { get; set; }
    }
}
