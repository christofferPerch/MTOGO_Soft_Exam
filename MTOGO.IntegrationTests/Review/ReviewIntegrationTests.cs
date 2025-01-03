using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MTOGO.Services.ReviewAPI.Models.Dto;

namespace MTOGO.IntegrationTests.Review
{
    public class ReviewIntegrationTests : IClassFixture<CustomReviewWebApplicationFactory<MTOGO.Services.ReviewAPI.Program>>
    {
        private readonly HttpClient _client;

        public ReviewIntegrationTests(CustomReviewWebApplicationFactory<MTOGO.Services.ReviewAPI.Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task AddRestaurantReview_ShouldReturnOk()
        {
            var review = new RestaurantReviewDto
            {
                CustomerId = "TestCustomer1",
                FoodRating = 5,
                Comments = "Excellent service and food!",
                RestaurantId = 1
            };

            var response = await _client.PostAsJsonAsync("api/review/restaurant/add", review);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();
            responseData.Message.Should().Be("Restaurant Review added successfully.");
            responseData.Result.Should().NotBeNull(); 
        }

        [Fact]
        public async Task GetRestaurantReview_ShouldReturnReviews()
        {
            var restaurantId = 1;

            var response = await _client.GetAsync($"api/review/restaurant/{restaurantId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();
            responseData.Message.Should().Be("Restaurant reviews retrieved successfully.");

            var reviews = JsonSerializer.Deserialize<List<RestaurantReviewDto>>(
                responseData.Result!.ToString(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            reviews.Should().NotBeNull();
            reviews!.Should().NotBeEmpty("There should be at least one review for the specified restaurant.");
        }

        [Fact]
        public async Task GetRestaurantReview_ShouldReturnNotFound_WhenNoReviewsExist()
        {
            var restaurantId = 9999; 

            var response = await _client.GetAsync($"api/review/restaurant/{restaurantId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeFalse();
            responseData.Message.Should().Be("No reviews found for the specified restaurant.");
        }

        [Fact]
        public async Task DeleteRestaurantReview_ShouldReturnOk()
        {
            var review = new RestaurantReviewDto
            {
                CustomerId = "TestCustomerToDelete",
                FoodRating = 3,
                Comments = "Average experience.",
                RestaurantId = 2
            };

            var addResponse = await _client.PostAsJsonAsync("api/review/restaurant/add", review);
            var addResponseData = await addResponse.Content.ReadFromJsonAsync<ResponseDto>();
            var reviewId = JsonSerializer.Deserialize<int>(addResponseData!.Result!.ToString());

            var deleteResponse = await _client.DeleteAsync($"api/review/restaurant/{reviewId}");

            deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var deleteResponseData = await deleteResponse.Content.ReadFromJsonAsync<ResponseDto>();
            deleteResponseData.Should().NotBeNull();
            deleteResponseData!.IsSuccess.Should().BeTrue();
            deleteResponseData.Message.Should().Be("Restaurant Review deleted successfully.");
        }

        [Fact]
        public async Task DeleteRestaurantReview_ShouldReturnNotFound_WhenReviewDoesNotExist()
        {
            var nonExistentReviewId = 99999; 

            var response = await _client.DeleteAsync($"api/review/restaurant/{nonExistentReviewId}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeFalse();
            responseData.Message.Should().Be("Restaurant Review not found.");
        }
    }
}
