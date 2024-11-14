using MTOGO.Web.Models.ShoppingCart;
using MTOGO.Web.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MTOGO.Web.Controllers
{
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly IShoppingCartService _shoppingCartService;

        public ShoppingCartController(IShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(string userId)
        {
            var response = await _shoppingCartService.GetCartAsync(userId);
            if (response != null && response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddItemToCart([FromBody] Cart cart)
        {
            if (cart?.UserId == null || cart.Items == null || cart.Items.Count == 0)
            {
                return BadRequest("Invalid cart data.");
            }

            var response = await _shoppingCartService.AddItemToCartAsync(cart.UserId, cart.Items[0]); // Adding one item at a time
            if (response != null && response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveItemFromCart(string userId, int menuItemId)
        {
            var response = await _shoppingCartService.RemoveItemFromCartAsync(userId, menuItemId);
            if (response != null && response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response);
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
