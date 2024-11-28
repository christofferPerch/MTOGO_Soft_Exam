using MTOGO.Web.Models;

namespace MTOGO.Web.Services.IServices
{
    public interface IRestaurantService
    {
        Task<ResponseDto?> GetAllRestaurantsAsync();

        Task<ResponseDto?> SearchRestaurants(string location);

        Task<ResponseDto?> UniqueCities();
        Task<ResponseDto?> GetRestaurantByIdAsync(int id);
        Task<ResponseDto?> GetCartDetailsAsync(int restaurantId, int menuItemId);

    }
}
