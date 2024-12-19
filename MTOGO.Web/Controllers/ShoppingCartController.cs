using Microsoft.AspNetCore.Mvc;
using MTOGO.Web.Models;
using MTOGO.Web.Models.ShoppingCart;
using MTOGO.Web.Services.IServices;
using Newtonsoft.Json;

namespace MTOGO.Web.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IShoppingCartService _shoppingCartService;

        public ShoppingCartController(IShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ResponseDto
                {
                    IsSuccess = false,
                    Message = "User ID is required."
                });
            }

            var response = await _shoppingCartService.GetCartAsync(userId);

            if (response != null && response.IsSuccess)
            {
                if (response.Result != null)
                {
                    var cartResponse = JsonConvert.DeserializeObject<CartResponseMessageDto>(response.Result.ToString());

                    // Check if the cart is empty
                    if (cartResponse.Items == null || !cartResponse.Items.Any())
                    {
                        return Ok(new
                        {
                            userId = cartResponse.UserId,
                            items = new List<object>(), // Return an empty items list
                            correlationId = cartResponse.CorrelationId
                        });
                    }

                    // Return the populated cart
                    return Ok(new
                    {
                        userId = cartResponse.UserId,
                        items = cartResponse.Items,
                        correlationId = cartResponse.CorrelationId
                    });
                }
            }

            return BadRequest(new ResponseDto
            {
                IsSuccess = false,
                Message = response?.Message ?? "Failed to retrieve cart."
            });
        }

        [HttpPost]
        public async Task<IActionResult> SetCart([FromBody] Cart cart)
        {
            if (cart == null || string.IsNullOrEmpty(cart.UserId) || cart.Items == null || !cart.Items.Any())
            {
                return BadRequest(new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Invalid cart or missing user ID/items."
                });
            }

            try
            {
                var updatedCart = await _shoppingCartService.SetCart(cart);

                if (updatedCart == null)
                {
                    return Ok(new ResponseDto
                    {
                        IsSuccess = true,
                        Message = "Cart was empty and has been removed."
                    });
                }

                return Ok(new ResponseDto
                {
                    IsSuccess = true,
                    Result = updatedCart,
                    Message = "Cart updated successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto
                {
                    IsSuccess = false,
                    Message = $"An error occurred while setting the cart. {ex.Message}"
                });
            }
        }



        [HttpPost("add")]
        public async Task<IActionResult> AddItemToCart([FromBody] Cart cart)
        {
            if (cart?.UserId == null || cart.Items == null || cart.Items.Count == 0)
            {
                return BadRequest("Invalid cart data.");
            }

            var response = await _shoppingCartService.AddItemToCartAsync(cart.UserId, cart.Items[0]);
            if (response != null && response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }



        [HttpDelete]
        public async Task<IActionResult> RemoveItemFromCart(string userId, int menuItemId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ResponseDto
                {
                    IsSuccess = false,
                    Message = "User ID is required."
                });
            }

            var response = await _shoppingCartService.RemoveItemFromCartAsync(userId, menuItemId);

            if (response != null && response.IsSuccess)
            {
                return Ok(response);
            }

            return BadRequest(new ResponseDto
            {
                IsSuccess = false,
                Message = response?.Message ?? "Failed to remove cart item."
            });
        }


        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart(string userId)
        {
            var response = await _shoppingCartService.ClearCartAsync(userId);
            if (response != null && response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

    }
}
