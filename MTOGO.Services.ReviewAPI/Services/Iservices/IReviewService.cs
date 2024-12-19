using MTOGO.Services.ReviewAPI.Models;
using MTOGO.Services.ReviewAPI.Models.Dto;

namespace MTOGO.Services.ReviewAPI.Services.IServices
{
    public interface IReviewService
    {
        Task<int> AddRestaurantReviewAsync(RestaurantReviewDto restaurantReviewDto);
        Task<List<RestaurantReview>?> GetRestaurantReviewAsync(int restaurantId);
        Task<bool> DeleteRestaurantReviewAsync(int id);
    }
}
