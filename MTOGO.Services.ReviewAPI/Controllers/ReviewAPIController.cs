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

        [HttpPost("delivery")]
        public async Task<IActionResult> AddDeliveryReview([FromBody] DeliveryReviewDto deliveryReviewDto) {
            var id = await _reviewService.AddDeliveryReviewAsync(deliveryReviewDto);
            return Ok(new { Message = "Delivery Review added successfully.", ReviewId = id });
        }

        [HttpPost("restaurant")]
        public async Task<IActionResult> AddRestaurantReview([FromBody] RestaurantReviewDto restaurantReviewDto) {
            var id = await _reviewService.AddRestaurantReviewAsync(restaurantReviewDto);
            return Ok(new { Message = "Restaurant Review added successfully.", ReviewId = id });
        }

        [HttpGet("delivery/{id}")]
        public async Task<IActionResult> GetDeliveryReview(int id) {
            var review = await _reviewService.GetDeliveryReviewAsync(id);
            return review == null ? NotFound() : Ok(review);
        }

        [HttpGet("restaurant/{id}")]
        public async Task<IActionResult> GetRestaurantReview(int id) {
            var review = await _reviewService.GetRestaurantReviewAsync(id);
            return review == null ? NotFound() : Ok(review);
        }

        [HttpDelete("delivery/{id}")]
        public async Task<IActionResult> DeleteDeliveryReview(int id) {
            var result = await _reviewService.DeleteDeliveryReviewAsync(id);
            return result
                ? Ok(new { Message = "Delivery Review deleted successfully." })
                : NotFound(new { Message = "Delivery Review not found." });
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
