namespace MTOGO.Services.ReviewAPI.Models.Dto {
    public class RestaurantReviewDto {
        public string CustomerId { get; set; } = string.Empty!;
        public int FoodRating { get; set; }
        public string? Comments { get; set; }
    }
}
