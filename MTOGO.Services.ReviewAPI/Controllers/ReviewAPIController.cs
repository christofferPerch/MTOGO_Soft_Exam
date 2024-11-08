using MTOGO.Services.ReviewAPI.Models.Dto;
using MTOGO.Services.ReviewAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace MTOGO.Services.ReviewAPI.Controllers {
    [Route("api/review")]
    [ApiController]
    public class ReviewController : ControllerBase {
        private readonly IReviewService _ReviewService;

        public ReviewController(IReviewService ReviewService) {
            _ReviewService = ReviewService;
        }

        [HttpPost("delivery")]
        public async Task<IActionResult> AddDeliveryReview([FromBody] DeliveryReviewDto deliveryReviewDto) {
            var id = await _ReviewService.AddDeliveryReviewAsync(deliveryReviewDto);
            return Ok(new { Message = "Delivery Review added successfully.", ReviewId = id });
        }

        [HttpPost("restaurant")]
        public async Task<IActionResult> AddRestaurantReview([FromBody] RestaurantReviewDto restaurantReviewDto) {
            var id = await _ReviewService.AddRestaurantReviewAsync(restaurantReviewDto);
            return Ok(new { Message = "Restaurant Review added successfully.", ReviewId = id });
        }

        [HttpGet("delivery/{id}")]
        public async Task<IActionResult> GetDeliveryReview(int id) {
            var Review = await _ReviewService.GetDeliveryReviewAsync(id);
            return Review == null ? NotFound() : Ok(Review);
        }

        [HttpGet("restaurant/{id}")]
        public async Task<IActionResult> GetRestaurantReview(int id) {
            var Review = await _ReviewService.GetRestaurantReviewAsync(id);
            return Review == null ? NotFound() : Ok(Review);
        }
    }
}
