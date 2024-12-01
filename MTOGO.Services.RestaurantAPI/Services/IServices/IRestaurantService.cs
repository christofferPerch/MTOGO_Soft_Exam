using MTOGO.Services.RestaurantAPI.Models.Dto;

namespace MTOGO.Services.RestaurantAPI.Services.IServices {
    public interface IRestaurantService {
        Task<int> AddRestaurant(AddRestaurantDto restaurant);
        Task<int> AddMenuItem(AddMenuItemDto menuItemDto);
        Task<int> UpdateRestaurant(UpdateRestaurantDto updateRestaurantDto);
        Task<int> RemoveMenuItem(int restaurantId, int menuItemId);
        Task<int> DeleteRestaurant(int id);
        Task<RestaurantDto?> GetRestaurantById(int id);
        Task<List<RestaurantDto>> GetAllRestaurants();
        Task<List<RestaurantDto>> FindRestaurants(string? location, string? foodCategory); // New generalized search method
        Task<List<string>> GetUniqueCitiesAsync();
        Task<CartDetailsDto?> GetCartDetails(int restaurantId, int menuItemId);
    }
}
