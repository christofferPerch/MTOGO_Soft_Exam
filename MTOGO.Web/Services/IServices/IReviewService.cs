using MTOGO.Web.Models;
using MTOGO.Web.Models.Review;

namespace MTOGO.Web.Services.IServices
{
    public interface IReviewService
    {
        Task<ResponseDto?> AddRestaurantReviewAsync(RestaurantReviewDto restaurantReviewDto);
        Task<ResponseDto?> GetRestaurantReviewAsync(int id);
        Task<ResponseDto?> DeleteRestaurantReviewAsync(int id);
    }
}
