using Microsoft.AspNetCore.Mvc;
using MTOGO.Web.Models;
using MTOGO.Web.Services.IServices;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MTOGO.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRestaurantService _restaurantService;

        public HomeController(ILogger<HomeController> logger, IRestaurantService restaurantService)
        {
            _logger = logger;
            _restaurantService = restaurantService;
        }

        public async Task<IActionResult> Index()
        {
            var cities = new List<string>();
            var response = await _restaurantService.UniqueCities();

            if (response != null && response.IsSuccess && response.Result != null)
            {
                cities = JsonConvert.DeserializeObject<List<string>>(response.Result.ToString());
            }
            else
            {
                TempData["error"] = response?.Message ?? "Failed to load cities.";
            }

            return View(cities);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
