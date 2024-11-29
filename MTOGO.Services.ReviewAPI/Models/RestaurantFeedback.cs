namespace MTOGO.Services.ReviewAPI.Models {
    public class RestaurantReview {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty!;
        public int FoodRating { get; set; }
        public string? Comments { get; set; }
        public DateTime ReviewTimestamp { get; set; }

        public int RestaurantId { get; set; }
    }
}
