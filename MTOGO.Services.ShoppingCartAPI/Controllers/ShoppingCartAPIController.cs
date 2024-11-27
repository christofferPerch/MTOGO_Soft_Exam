using Microsoft.AspNetCore.Mvc;
using MTOGO.MessageBus;
using MTOGO.Services.ShoppingCartAPI.Models;
using MTOGO.Services.ShoppingCartAPI.Models.Dto;
using MTOGO.Services.ShoppingCartAPI.Services.IServices;

namespace MTOGO.Services.ShoppingCartAPI.Controllers
{
    [Route("api/shoppingcart")]
    [ApiController]
    public class ShoppingCartAPIController : ControllerBase
    {
        private readonly IShoppingCartService _cartService;
        protected ResponseDto _response;

        public ShoppingCartAPIController(IShoppingCartService cartService, IMessageBus messageBus)
        {
            _cartService = cartService;
            _response = new ResponseDto();
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _response.IsSuccess = false;
                    _response.Message = "User ID is required.";
                    return BadRequest(_response);
                }

                var cart = await _cartService.GetCart(userId);
                if (cart == null || cart.Items.Count == 0)
                {
                    // Respond with an empty cart structure if no items exist
                    _response.IsSuccess = true;
                    _response.Result = new Cart
                    {
                        UserId = userId,
                        Items = new List<CartItem>() // Empty items list
                    };
                    _response.Message = "Cart is empty.";
                    return Ok(_response);
                }

                _response.IsSuccess = true;
                _response.Result = cart;
                _response.Message = "Cart retrieved successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while retrieving the cart: " + ex.Message;
                return StatusCode(500, _response);
            }
        }

        [HttpPost("set")]
        public async Task<IActionResult> SetCart([FromBody] Cart cart)
        {
            if (cart == null || string.IsNullOrEmpty(cart.UserId))
            {
                return BadRequest(new ResponseDto
                {
                    IsSuccess = false,
                    Message = "Invalid cart or user ID."
                });
            }

            try
            {
                var updatedCart = await _cartService.SetCart(cart);

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


        [HttpPost("create")]
        public async Task<IActionResult> CreateCart([FromBody] Cart cart)
        {
            try
            {
                var createdCart = await _cartService.CreateCart(cart);
                _response.Result = createdCart;
                _response.Message = "Cart created successfully.";
                return CreatedAtAction(nameof(GetCart), new { userId = createdCart.UserId }, _response);
            }
            catch (InvalidOperationException ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
                return Conflict(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while creating the cart." + ex;
                return StatusCode(500, _response);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCart([FromBody] Cart cart)
        {
            try
            {
                var updatedCart = await _cartService.UpdateCart(cart);
                _response.Result = updatedCart;
                _response.Message = "Cart updated successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while updating the cart." + ex;
                return StatusCode(500, _response);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveMenuItem(string userId, int menuItemId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _response.IsSuccess = false;
                _response.Message = "User ID is required.";
                return BadRequest(_response);
            }

            try
            {
                var result = await _cartService.RemoveMenuItem(userId, menuItemId);
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Item not found in the cart.";
                    return NotFound(_response);
                }

                var updatedCart = await _cartService.GetCart(userId);
                _response.IsSuccess = true;
                _response.Result = updatedCart;
                _response.Message = "Item removed successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while removing the item. " + ex.Message;
                return StatusCode(500, _response);
            }
        }




        [HttpDelete("{userId}")]
        public async Task<IActionResult> RemoveCart(string userId)
        {
            try
            {
                var result = await _cartService.RemoveCart(userId);
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Cart not found or could not be removed.";
                    return NotFound(_response);
                }

                _response.Message = "Cart removed successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while removing the cart." + ex;
                return StatusCode(500, _response);
            }
        }
    }
}
