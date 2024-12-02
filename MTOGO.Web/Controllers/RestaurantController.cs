using Microsoft.AspNetCore.Mvc;
using MTOGO.Web.Models.Restaurant;
using MTOGO.Web.Models.Review;
using MTOGO.Web.Services.IServices;
using Newtonsoft.Json;

namespace MTOGO.Web.Controllers
{
    public class RestaurantController : Controller
    {
        private readonly IRestaurantService _restaurantService;
        private readonly IReviewService _reviewService;

        public RestaurantController(IRestaurantService restaurantService, IReviewService reviewService)
        {
            _restaurantService = restaurantService;
            _reviewService = reviewService;
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
        public async Task<IActionResult> Search(string location, int? foodCategory = null)
        {
            var response = await _restaurantService.SearchRestaurants(location, foodCategory);
            if (response != null && response.IsSuccess && response.Result != null)
            {
                var restaurants = JsonConvert.DeserializeObject<List<RestaurantDto>>(response.Result.ToString());

                var cityName = restaurants.FirstOrDefault()?.Address?.City ?? location;
                ViewBag.SelectedCity = cityName;
                ViewBag.FoodCategory = foodCategory;
                return View(restaurants);
            }

            TempData["error"] = response?.Message ?? "No restaurants found.";
            ViewBag.SelectedCity = location;
            ViewBag.FoodCategory = foodCategory;
            return View(new List<RestaurantDto>());
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

                var reviewResponse = await _reviewService.GetRestaurantReviewAsync(id); // Assuming this fetches all reviews
                if (reviewResponse != null && reviewResponse.IsSuccess && reviewResponse.Result != null)
                {
                    var reviews = JsonConvert.DeserializeObject<List<RestaurantReview>>(reviewResponse.Result.ToString());
                    ViewBag.Reviews = reviews;

                    // Calculate average rating
                    if (reviews.Any())
                    {
                        ViewBag.AverageRating = reviews.Average(r => r.FoodRating);
                    }
                    else
                    {
                        ViewBag.AverageRating = 0; // No reviews
                    }
                }

                return View(restaurant);
            }

            TempData["error"] = response?.Message ?? "Failed to load restaurant details.";
            return RedirectToAction("Search");
        }





        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] RestaurantReviewDto reviewDto)
        {
            try
            {
                if (reviewDto == null || reviewDto.RestaurantId <= 0)
                {
                    return Json(new { success = false, message = "Invalid review data." });
                }

                var response = await _reviewService.AddRestaurantReviewAsync(reviewDto);
                if (response != null && response.IsSuccess)
                {
                    return Json(new { success = true, message = "Review added successfully." });
                }

                return Json(new { success = false, message = response?.Message ?? "Failed to add review." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred while adding the review: {ex.Message}" });
            }
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var response = await _reviewService.DeleteRestaurantReviewAsync(id);
            if (response != null && response.IsSuccess)
            {
                return Json(new { success = true, message = "Review deleted successfully." });
            }

            return Json(new { success = false, message = response?.Message ?? "Failed to delete review." });
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
