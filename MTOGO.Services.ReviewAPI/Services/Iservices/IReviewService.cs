using MTOGO.Services.ReviewAPI.Models.Dto;
using MTOGO.Services.ReviewAPI.Models;

namespace MTOGO.Services.ReviewAPI.Services.IServices {
    public interface IReviewService {
        Task<int> AddRestaurantReviewAsync(RestaurantReviewDto restaurantReviewDto);
        Task<RestaurantReview?> GetRestaurantReviewAsync(int id);
        Task<bool> DeleteRestaurantReviewAsync(int id); 
    }
}
