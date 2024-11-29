using MTOGO.Services.ReviewAPI.Models.Dto;
using MTOGO.Services.ReviewAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace MTOGO.Services.ReviewAPI.Controllers {
    [Route("api/review")]
    [ApiController]
    public class ReviewController : ControllerBase {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService) {
            _reviewService = reviewService;
        }
        [HttpPost("restaurant")]
        public async Task<IActionResult> AddRestaurantReview([FromBody] RestaurantReviewDto restaurantReviewDto) {
            if (restaurantReviewDto == null || restaurantReviewDto.RestaurantId <= 0) {
                return BadRequest(new { Message = "Invalid review data. Please provide a valid RestaurantId." });
            }

            var id = await _reviewService.AddRestaurantReviewAsync(restaurantReviewDto);
            return Ok(new { Message = "Restaurant Review added successfully.", ReviewId = id });
        }

        [HttpGet("restaurant/{id}")]
        public async Task<IActionResult> GetRestaurantReview(int id) {
            var review = await _reviewService.GetRestaurantReviewAsync(id);
            return review == null
                ? NotFound(new { Message = "Restaurant Review not found." })
                : Ok(review);
        }

        [HttpDelete("restaurant/{id}")]
        public async Task<IActionResult> DeleteRestaurantReview(int id) {
            var result = await _reviewService.DeleteRestaurantReviewAsync(id);
            return result
                ? Ok(new { Message = "Restaurant Review deleted successfully." })
                : NotFound(new { Message = "Restaurant Review not found." });
        }
    }
}
