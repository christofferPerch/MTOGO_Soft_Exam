using MTOGO.Services.ReviewAPI.Models.Dto;
using MTOGO.Services.ReviewAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace MTOGO.Services.ReviewAPI.Controllers
{
    [ApiController]
    [Route("api/review")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        protected ResponseDto _response;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
            _response = new ResponseDto();
        }

        [HttpPost("restaurant/add")]
        public async Task<IActionResult> AddRestaurantReview([FromBody] RestaurantReviewDto restaurantReviewDto)
        {
            try
            {
                if (restaurantReviewDto == null || restaurantReviewDto.RestaurantId <= 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Invalid review data. Please provide a valid RestaurantId.";
                    return BadRequest(_response);
                }

                var id = await _reviewService.AddRestaurantReviewAsync(restaurantReviewDto);
                _response.IsSuccess = true;
                _response.Result = id;
                _response.Message = "Restaurant Review added successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = $"An error occurred while adding the restaurant review: {ex.Message}";
                return StatusCode(500, _response);
            }
        }


        [HttpGet("restaurant/{id}")]
        public async Task<IActionResult> GetRestaurantReview(int id)
        {
            try
            {
                var reviews = await _reviewService.GetRestaurantReviewAsync(id);
                if (reviews == null || !reviews.Any())
                {
                    _response.IsSuccess = false;
                    _response.Message = "No reviews found for the specified restaurant.";
                    return NotFound(_response);
                }

                _response.IsSuccess = true;
                _response.Result = reviews;
                _response.Message = "Restaurant reviews retrieved successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while retrieving the restaurant reviews: " + ex.Message;
                return StatusCode(500, _response);
            }
        }


        [HttpDelete("restaurant/{id}")]
        public async Task<IActionResult> DeleteRestaurantReview(int id)
        {
            try
            {
                var result = await _reviewService.DeleteRestaurantReviewAsync(id);
                if (!result)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Restaurant Review not found.";
                    return NotFound(_response);
                }

                _response.IsSuccess = true;
                _response.Message = "Restaurant Review deleted successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = "An error occurred while deleting the restaurant review: " + ex.Message;
                return StatusCode(500, _response);
            }
        }
    }
}
