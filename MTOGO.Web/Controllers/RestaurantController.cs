using Microsoft.AspNetCore.Mvc;
using MTOGO.Web.Models.Restaurant;
using MTOGO.Web.Services.IServices;
using Newtonsoft.Json;

namespace MTOGO.Web.Controllers
{
    public class RestaurantController : Controller
    {
        private readonly IRestaurantService _restaurantService;

        public RestaurantController(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var response = await _restaurantService.GetAllRestaurantsAsync();
            if (response != null && response.IsSuccess)
            {
                var restaurantList = JsonConvert.DeserializeObject<IEnumerable<RestaurantDto>>(response.Result.ToString());
                return View(restaurantList);
            }

            TempData["error"] = response?.Message ?? "Failed to load restaurants.";
            return View(new List<RestaurantDto>());
        }

    }
}
