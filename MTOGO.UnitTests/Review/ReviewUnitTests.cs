using Moq;
using MTOGO.Services.DataAccess;
using MTOGO.Services.ReviewAPI.Models;
using MTOGO.Services.ReviewAPI.Models.Dto;
using MTOGO.Services.ReviewAPI.Services;
using Xunit;
using Microsoft.Extensions.Logging;

namespace MTOGO.UnitTests.Review
{
    public class ReviewUnitTests
    {
        private readonly Mock<IDataAccess> _dataAccessMock;
        private readonly Mock<ILogger<ReviewService>> _loggerMock;
        private readonly ReviewService _reviewService;

        public ReviewUnitTests()
        {
            _dataAccessMock = new Mock<IDataAccess>();
            _loggerMock = new Mock<ILogger<ReviewService>>();
            _reviewService = new ReviewService(_dataAccessMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task AddRestaurantReviewAsync_ValidData_ReturnsReviewId()
        {
            var reviewDto = new RestaurantReviewDto
            {
                CustomerId = "test",
                RestaurantId = 101,
                FoodRating = 4,
                Comments = "Great food!"
            };

            _dataAccessMock.Setup(d => d.InsertAndGetId<int>(It.IsAny<string>(), It.IsAny<object>()))
                           .ReturnsAsync(123);

            var result = await _reviewService.AddRestaurantReviewAsync(reviewDto);

            Assert.Equal(123, result);
            _dataAccessMock.Verify(d => d.InsertAndGetId<int>(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task AddRestaurantReviewAsync_InvalidData_ThrowsArgumentException()
        {
            var reviewDto = new RestaurantReviewDto
            {
                CustomerId = null, 
                RestaurantId = 101,
                FoodRating = 4,
                Comments = "Great food!"
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _reviewService.AddRestaurantReviewAsync(reviewDto));
            Assert.Equal("Invalid review data. CustomerId must be provided.", exception.Message);
        }


        [Fact]
        public async Task GetRestaurantReviewAsync_ValidRestaurantId_ReturnsReviews()
        {
            var restaurantId = 101;
            var expectedReviews = new List<RestaurantReview>
            {
                new RestaurantReview { Id = 1, CustomerId = "test", RestaurantId = 101, FoodRating = 4, Comments = "Great food!", ReviewTimestamp = DateTime.Now },
                new RestaurantReview { Id = 2, CustomerId = "test", RestaurantId = 101, FoodRating = 3, Comments = "Good service!", ReviewTimestamp = DateTime.Now }
            };

            _dataAccessMock.Setup(d => d.GetAll<RestaurantReview>(It.IsAny<string>(), It.IsAny<object>()))
                           .ReturnsAsync(expectedReviews);

            var result = await _reviewService.GetRestaurantReviewAsync(restaurantId);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(101, result.First().RestaurantId);
            _dataAccessMock.Verify(d => d.GetAll<RestaurantReview>(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task GetRestaurantReviewAsync_InvalidRestaurantId_ReturnsEmptyList()
        {
            var restaurantId = 999; 

            _dataAccessMock.Setup(d => d.GetAll<RestaurantReview>(It.IsAny<string>(), It.IsAny<object>()))
                           .ReturnsAsync(new List<RestaurantReview>());

            var result = await _reviewService.GetRestaurantReviewAsync(restaurantId);

            Assert.NotNull(result);
            Assert.Empty(result);
            _dataAccessMock.Verify(d => d.GetAll<RestaurantReview>(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task DeleteRestaurantReviewAsync_ValidId_ReturnsTrue()
        {
            var reviewId = 123;

            _dataAccessMock.Setup(d => d.Delete(It.IsAny<string>(), It.IsAny<object>()))
                           .ReturnsAsync(1);

            var result = await _reviewService.DeleteRestaurantReviewAsync(reviewId);

            Assert.True(result);
            _dataAccessMock.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task DeleteRestaurantReviewAsync_InvalidId_ReturnsFalse()
        {
            var reviewId = 999; // Non-existent review ID

            _dataAccessMock.Setup(d => d.Delete(It.IsAny<string>(), It.IsAny<object>()))
                           .ReturnsAsync(0);

            var result = await _reviewService.DeleteRestaurantReviewAsync(reviewId);

            Assert.False(result);
            _dataAccessMock.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }
}
