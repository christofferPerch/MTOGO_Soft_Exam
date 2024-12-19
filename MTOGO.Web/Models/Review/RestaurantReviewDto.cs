namespace MTOGO.Web.Models.Review
{
    public class RestaurantReviewDto
    {
        public string CustomerId { get; set; } = string.Empty!;
        public int FoodRating { get; set; }
        public string? Comments { get; set; }
        public int RestaurantId { get; set; }
    }
}
