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

        [HttpGet]
        public async Task<IActionResult> Search(string location)
        {
            var response = await _restaurantService.SearchRestaurants(location);
            if (response != null && response.IsSuccess && response.Result != null)
            {
                var restaurants = JsonConvert.DeserializeObject<List<RestaurantDto>>(response.Result.ToString());
                ViewBag.SelectedCity = location; // Pass the selected city to the view for display
                return View(restaurants);
            }

            TempData["error"] = response?.Message ?? "No restaurants found.";
            return View(new List<RestaurantDto>()); // Return empty list if no results found
        }



        [HttpGet]
        public async Task<IActionResult> UniqueCities()
        {
            var response = await _restaurantService.UniqueCities();
            if (response != null && response.IsSuccess && response.Result != null)
            {
                var cities = JsonConvert.DeserializeObject<List<string>>(response.Result.ToString());
                return Json(cities); 
            }
            return Json(new List<string>()); 
        }

        [HttpGet("Restaurant/Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var response = await _restaurantService.GetRestaurantByIdAsync(id);
            if (response != null && response.IsSuccess && response.Result != null)
            {
                var restaurant = JsonConvert.DeserializeObject<RestaurantDto>(response.Result.ToString());
                return View(restaurant);
            }

            TempData["error"] = response?.Message ?? "Failed to load restaurant details.";
            return RedirectToAction("Search");
        }

        [HttpGet]
        public async Task<IActionResult> GetCartDetails(int restaurantId, int menuItemId)
        {
            var response = await _restaurantService.GetCartDetailsAsync(restaurantId, menuItemId);
            if (response != null && response.IsSuccess && response.Result != null)
            {
                var cartDetails = JsonConvert.DeserializeObject<CartDetailsDto>(response.Result.ToString());
                return Json(cartDetails); // Return as JSON, can be adjusted as needed for your use case
            }

            return Json(new { success = false, message = response?.Message ?? "Failed to load cart details." });
        }



    }
}
