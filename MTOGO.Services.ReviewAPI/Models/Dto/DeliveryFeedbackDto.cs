namespace MTOGO.Services.ReviewAPI.Models.Dto {
    public class DeliveryReviewDto {
        public int OrderId { get; set; }
        public string CustomerId { get; set; } = string.Empty!;
        public int DeliveryAgentId { get; set; }
        public int DeliveryExperienceRating { get; set; }
        public string? Comments { get; set; }
    }
}
