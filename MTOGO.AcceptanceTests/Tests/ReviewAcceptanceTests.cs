using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MTOGO.Services.ReviewAPI.Models;
using MTOGO.Services.ReviewAPI.Models.Dto;
using Newtonsoft.Json;

public class ReviewAcceptanceTests
{
    private readonly HttpClient _client;

    public ReviewAcceptanceTests()
    {
        _client = new HttpClient { BaseAddress = new Uri("http://localhost:5005/api/review/") };
    }

    private async Task<long> AddTestRestaurantReview()
    {
        var review = new
        {
            CustomerId = "test-customer",
            RestaurantId = 1,
            FoodRating = 5,
            Comments = "Great food!"
        };

        var response = await _client.PostAsJsonAsync("restaurant/add", review);
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Adding a test review should succeed");

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ResponseDto>(responseBody);
        result.Should().NotBeNull("Response should not be null");
        result.IsSuccess.Should().BeTrue("Response should indicate success");
        result.Result.Should().NotBeNull("Result should not be null");

        return Convert.ToInt64(result.Result);
    }

    private async Task DeleteTestRestaurantReview(long reviewId)
    {
        var response = await _client.DeleteAsync($"restaurant/{reviewId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Deleting the test review should succeed");
    }

    [Fact]
    public async Task CustomerCanAddRestaurantReview_ShouldSucceed()
    {
        var review = new
        {
            CustomerId = "test-customer",
            RestaurantId = 1,
            FoodRating = 5,
            Comments = "Excellent food and service!"
        };

        var response = await _client.PostAsJsonAsync("restaurant/add", review);
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Adding a review should succeed");

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ResponseDto>(responseBody);

        result.Should().NotBeNull("Response should not be null");
        result.IsSuccess.Should().BeTrue("Response should indicate success");
        result.Result.Should().NotBeNull("The result should not be null");
    }

    [Fact]
    public async Task CustomerCanRetrieveRestaurantReviews_ShouldReturnReviews()
    {
        var reviewId = await AddTestRestaurantReview();

        var response = await _client.GetAsync("restaurant/1");
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Retrieving reviews should succeed");

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ResponseDto>(responseBody);

        result.Should().NotBeNull("Response should not be null");
        result.IsSuccess.Should().BeTrue("Response should indicate success");

        var reviews = JsonConvert.DeserializeObject<List<RestaurantReview>>(result.Result.ToString());
        reviews.Should().NotBeNullOrEmpty("There should be at least one review");
        reviews.Should().ContainSingle(r => r.Id == reviewId, "The review we added should be present");

        await DeleteTestRestaurantReview(reviewId);
    }

    [Fact]
    public async Task CustomerCanDeleteRestaurantReview_ShouldSucceed()
    {
        var reviewId = await AddTestRestaurantReview();

        var response = await _client.DeleteAsync($"restaurant/{reviewId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Deleting the review should succeed");

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ResponseDto>(responseBody);

        result.Should().NotBeNull("Response should not be null");
        result.IsSuccess.Should().BeTrue("Response should indicate success");

        var validateResponse = await _client.GetAsync($"restaurant/1");
        var validateBody = await validateResponse.Content.ReadAsStringAsync();
        var validateResult = JsonConvert.DeserializeObject<ResponseDto>(validateBody);

        if (validateResult?.Result != null)
        {
            var reviews = JsonConvert.DeserializeObject<List<RestaurantReview>>(validateResult.Result.ToString());
            reviews.Should().NotContain(r => r.Id == reviewId, "The review should no longer exist");
        }
    }

    [Fact]
    public async Task RetrievingNonExistentReviews_ShouldReturnNotFound()
    {
        int invalidRestaurantId = -1;

        var response = await _client.GetAsync($"restaurant/{invalidRestaurantId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "Retrieving non-existent reviews should return 404");

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ResponseDto>(responseBody);

        result.Should().NotBeNull("Response should not be null");
        result.IsSuccess.Should().BeFalse("Response should indicate failure");
        result.Message.Should().Be("No reviews found for the specified restaurant.");
    }
}
