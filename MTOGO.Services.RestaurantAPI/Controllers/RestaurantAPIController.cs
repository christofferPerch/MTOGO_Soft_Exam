using Microsoft.AspNetCore.Mvc;
using MTOGO.Services.RestaurantAPI.Models.Dto;
using MTOGO.Services.RestaurantAPI.Services.IServices;

namespace MTOGO.Services.RestaurantAPI.Controllers
{
    [ApiController]
    [Route("api/restaurant")]
    public class RestaurantAPIController : ControllerBase
    {
        private readonly IRestaurantService _restaurantService;
        protected ResponseDto _response;

        #region Constructor
        public RestaurantAPIController(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
            _response = new();
        }
        #endregion

        #region Post Methods
        [HttpPost("add")]
        public async Task<IActionResult> AddRestaurant([FromBody] AddRestaurantDto restaurant)
        {

            try
            {
                if (restaurant == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Restaurant data is invalid.";
                    return BadRequest(_response);
                }

                var restaurantId = await _restaurantService.AddRestaurant(restaurant);
                _response.Result = restaurantId;
                _response.Message = "Restaurant added successfully.";

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while adding the restaurant." + ex;
                return StatusCode(500, _response);
            }
        }

        [HttpPost("addMenuItem")]
        public async Task<IActionResult> AddMenuItem([FromBody] AddMenuItemDto menuItemDto)
        {
            try
            {
                if (menuItemDto == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Menu item data is invalid.";
                    return BadRequest(_response);
                }

                var menuItemId = await _restaurantService.AddMenuItem(menuItemDto);
                if (menuItemId == 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "An error occurred while adding the menu item.";
                    return StatusCode(500, _response);
                }

                _response.Result = menuItemId;
                _response.Message = "Menu item added successfully.";

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while adding the menu item." + ex;
                return StatusCode(500, _response);
            }
        }
        #endregion

        #region Put Methods
        [HttpPut("updateRestaurant")]
        public async Task<IActionResult> UpdateRestaurant([FromBody] UpdateRestaurantDto updateRestaurantDto)
        {
            try
            {
                if (updateRestaurantDto == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Restaurant data is invalid.";
                    return BadRequest(_response);
                }

                var result = await _restaurantService.UpdateRestaurant(updateRestaurantDto);
                if (result == 1)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Restaurant not found.";
                    return NotFound(_response);
                }

                _response.Message = "Restaurant updated successfully.";

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while updating the restaurant." + ex;
                return StatusCode(500, _response);
            }
        }
        #endregion

        #region Delete Methods
        [HttpDelete("deleteSpecificMenuItem")]
        public async Task<IActionResult> RemoveMenuItem(int restaurantId, int menuItemId)
        {
            try
            {
                var result = await _restaurantService.RemoveMenuItem(restaurantId, menuItemId);
                if (result == 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Menu item not found.";
                    return NotFound(_response);
                }

                _response.Message = "Menu item removed successfully.";

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while removing the menu item." + ex;
                return StatusCode(500, _response);
            }
        }

        [HttpDelete("deleteSpecificRestaurant")]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            try
            {
                var result = await _restaurantService.DeleteRestaurant(id);
                if (result == 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Restaurant not found.";
                    return NotFound(_response);
                }

                _response.Message = "Restaurant deleted successfully.";

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while deleting the restaurant." + ex;
                return StatusCode(500, _response);
            }
        }
        #endregion

        #region Get Methods
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRestaurantById(int id)
        {
            try
            {
                var restaurant = await _restaurantService.GetRestaurantById(id);
                if (restaurant == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Restaurant not found.";
                    return NotFound(_response);
                }

                _response.Result = restaurant;
                _response.Message = "Restaurant retrieved successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while retrieving the restaurant." + ex;
                return StatusCode(500, _response);
            }
        }


        [HttpGet("allRestaurants")]
        public async Task<IActionResult> GetAllRestaurants()
        {
            try
            {
                var restaurants = await _restaurantService.GetAllRestaurants();
                _response.Result = restaurants;
                _response.Message = "All restaurants retrieved successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while retrieving all restaurants." + ex;
                return StatusCode(500, _response);
            }
        }

        [HttpGet("searchRestaurant")]
        public async Task<IActionResult> SearchRestaurants([FromQuery] string location, [FromQuery] int? foodCategory = null)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                _response.IsSuccess = false;
                _response.Message = "At least one search parameter (location or food category) is required.";
                return BadRequest(_response);
            }

            try
            {
                // Call the service method with the location and foodCategory parameters
                var restaurants = await _restaurantService.FindRestaurantsByLocation(location, foodCategory);
                _response.Result = restaurants;
                _response.Message = "Restaurants retrieved successfully.";
                return Ok(_response);
            } catch (Exception ex) {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while retrieving restaurants. " + ex.Message;
                return StatusCode(500, _response);
            }
        }


        [HttpGet("uniqueCities")]
        public async Task<IActionResult> GetUniqueCities()
        {
            try
            {
                var cities = await _restaurantService.GetUniqueCitiesAsync();
                if (cities != null && cities.Any())
                {
                    _response.Result = cities;
                    _response.Message = "Cities retrieved successfully.";
                    _response.IsSuccess = true;
                }
                else
                {
                    _response.Result = new List<string>();
                    _response.Message = "No cities found.";
                    _response.IsSuccess = true;
                }
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"An error occurred: {ex.Message}";
                return StatusCode(500, _response);
            }
        }

        [HttpGet("cartDetails")]
        public async Task<IActionResult> GetCartDetails([FromQuery] int restaurantId, [FromQuery] int menuItemId)
        {
            try
            {
                if (restaurantId <= 0 || menuItemId <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Invalid RestaurantId or MenuItemId.";
                    return BadRequest(_response);
                }

                var cartDetails = await _restaurantService.GetCartDetails(restaurantId, menuItemId);
                if (cartDetails == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Menu item not found for the provided RestaurantId and MenuItemId.";
                    return NotFound(_response);
                }

                _response.Result = cartDetails;
                _response.Message = "Cart details retrieved successfully.";
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"An error occurred while retrieving cart details: {ex.Message}";
                return StatusCode(500, _response);
            }
        }


        #endregion
    }
}
