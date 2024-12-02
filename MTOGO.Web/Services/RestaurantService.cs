using MTOGO.Web.Models;
using MTOGO.Web.Services.IServices;
using MTOGO.Web.Utility;

namespace MTOGO.Web.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IBaseService _baseService;

        public RestaurantService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> GetAllRestaurantsAsync()
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{SD.RestaurantAPIBase}/api/restaurant/allRestaurants"
            });
        }

        public async Task<ResponseDto?> SearchRestaurants(string location, int? foodCategory = null)
        {
            var url = $"{SD.RestaurantAPIBase}/api/restaurant/searchRestaurant?location={location}";

            if (foodCategory.HasValue)
            {
                url += $"&foodCategory={foodCategory.Value}";
            }

            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = url
            });
        }


        public async Task<ResponseDto?> UniqueCities()
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{SD.RestaurantAPIBase}/api/restaurant/uniqueCities"
            });
        }

        public async Task<ResponseDto?> GetRestaurantByIdAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{SD.RestaurantAPIBase}/api/restaurant/{id}" 
            });
        }

        public async Task<ResponseDto?> GetCartDetailsAsync(int restaurantId, int menuItemId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{SD.RestaurantAPIBase}/api/restaurant/cartDetails?restaurantId={restaurantId}&menuItemId={menuItemId}"
            });
        }


    }
}
