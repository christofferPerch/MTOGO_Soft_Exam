using Microsoft.AspNetCore.Mvc;
using MTOGO.Web.Models;
using MTOGO.Web.Models.Order;
using MTOGO.Web.Models.Restaurant;
using MTOGO.Web.Models.ShoppingCart;
using MTOGO.Web.Services.IServices;
using Newtonsoft.Json;

namespace MTOGO.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderService _orderService;
        private readonly IRestaurantService _restaurantService;


        public OrderController(IShoppingCartService shoppingCartService, IOrderService orderService, IRestaurantService restaurantService)
        {
            _shoppingCartService = shoppingCartService;
            _orderService = orderService;
            _restaurantService = restaurantService;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirst("UserId")?.Value; // Get the logged-in user's ID
            if (string.IsNullOrEmpty(userId))
            {
                TempData["error"] = "User not logged in.";
                return RedirectToAction("Index", "Home");
            }

            var response = await _shoppingCartService.GetCartAsync(userId); // Call service to get cart
            if (response != null && response.IsSuccess && response.Result != null)
            {
                // Deserialize the response into CartResponseMessageDto
                var cartResponse = JsonConvert.DeserializeObject<CartResponseMessageDto>(response.Result.ToString());
                var cartItems = cartResponse?.Items ?? new List<OrderItemDto>();

                return View(cartItems); // Pass List<OrderItemDto> to the view
            }

            TempData["error"] = response?.Message ?? "Failed to load cart details.";
            return RedirectToAction("Index", "Home");
        }




        [HttpPost]
        public async Task<IActionResult> ProcessPayment(string userId, string cardNumber, string expiryDate, string cvv)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(cardNumber) || string.IsNullOrEmpty(expiryDate) || string.IsNullOrEmpty(cvv))
            {
                TempData["error"] = "Invalid payment details.";
                return RedirectToAction("Checkout");
            }

            // Create the payment request payload
            var paymentRequest = new PaymentRequestDto
            {
                UserId = userId,
                Items = new List<CartItem>(), // Leave empty; Order API will fetch the cart details
                TotalAmount = 0,              // Let the Order API calculate this
                CardNumber = cardNumber,
                ExpiryDate = expiryDate,
                CVV = cvv
            };

            // Send the payment request to the Order API
            var response = await _orderService.ProcessPaymentAsync(paymentRequest);

            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Payment processed successfully.";
                return RedirectToAction("Confirmation");
            }

            TempData["error"] = response?.Message ?? "Failed to process payment.";
            return RedirectToAction("Checkout");
        }

        [HttpGet]
        public async Task<IActionResult> GetCartDetails(int restaurantId, int menuItemId)
        {
            var response = await _restaurantService.GetCartDetailsAsync(restaurantId, menuItemId);
            if (response != null && response.IsSuccess && response.Result != null)
            {
                var cartDetails = JsonConvert.DeserializeObject<CartDetailsDto>(response.Result.ToString());
                return Json(cartDetails); // Return as JSON for your front-end to use
            }

            return Json(new { success = false, message = response?.Message ?? "Failed to load cart details." });
        }



        [HttpGet]
        public IActionResult Confirmation()
        {
            return View(); // Simple confirmation view
        }
    }
}
