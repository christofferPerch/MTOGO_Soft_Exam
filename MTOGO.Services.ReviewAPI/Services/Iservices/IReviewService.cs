using MTOGO.Services.ReviewAPI.Models.Dto;
using MTOGO.Services.ReviewAPI.Models;

namespace MTOGO.Services.ReviewAPI.Services.IServices {
    public interface IReviewService {
        Task<int> AddDeliveryReviewAsync(DeliveryReviewDto deliveryReviewDto);
        Task<int> AddRestaurantReviewAsync(RestaurantReviewDto restaurantReviewDto);
        Task<DeliveryReview?> GetDeliveryReviewAsync(int id);
        Task<RestaurantReview?> GetRestaurantReviewAsync(int id);
        Task<bool> DeleteDeliveryReviewAsync(int id); 
        Task<bool> DeleteRestaurantReviewAsync(int id); 
    }
}
