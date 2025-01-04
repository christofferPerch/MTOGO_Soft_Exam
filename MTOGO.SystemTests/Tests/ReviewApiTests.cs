using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using System.Text.Json;

public class ReviewAPITests {
    private readonly HttpClient _client;

    public ReviewAPITests() {
        _client = new HttpClient { BaseAddress = new Uri("http://localhost:5005/api/review/") }; // Review API URL
    }

    [Fact]
    public async Task AddRestaurantReview_ShouldReturnSuccess() {
        var review = new {
            customerId = "test-customer",
            restaurantId = 1,
            foodRating = 5,
            comments = "Excellent food and service!"
        };

        var response = await _client.PostAsJsonAsync("restaurant/add", review);

        response.StatusCode.Should().Be(HttpStatusCode.OK, "Adding a valid review should succeed");

        var jsonResponse = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        jsonResponse.RootElement.GetProperty("isSuccess").GetBoolean().Should().BeTrue("Response should indicate success");
        jsonResponse.RootElement.GetProperty("result").GetInt32().Should().BeGreaterThan(0, "A valid review ID should be returned");
    }

    [Fact]
    public async Task DeleteRestaurantReview_ShouldReturnSuccess() {
        // Step 1: Create a new review
        var newReview = new {
            customerId = "test-customer",
            foodRating = 5,
            comments = "Temporary review for deletion test",
            restaurantId = 1 // Existing restaurant ID
        };

        // Make POST request to add a new review
        var createResponse = await _client.PostAsJsonAsync("/api/review/restaurant/add", newReview);

        // Assert the response from the create operation
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Creating a review should succeed");

        var createResponseBody = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var reviewId = createResponseBody.GetProperty("result").GetInt32();

        // Step 2: Delete the created review
        var deleteResponse = await _client.DeleteAsync($"/api/review/restaurant/{reviewId}");

        // Assert the response from the delete operation
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Deleting a valid review should succeed");

        var deleteResponseBody = await deleteResponse.Content.ReadFromJsonAsync<JsonElement>();
        var isSuccess = deleteResponseBody.GetProperty("isSuccess").GetBoolean();
        isSuccess.Should().BeTrue("Response should indicate success");
    }




    [Fact]
    public async Task GetRestaurantReview_ShouldReturnReviews() {
        int restaurantId = 1; // Pre-existing restaurant ID with reviews

        var response = await _client.GetAsync($"/api/review/restaurant/{restaurantId}");

        // Assert the response
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Fetching reviews for a valid restaurant ID should succeed");

        var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();

        // Extract and assert the 'isSuccess' property
        bool isSuccess = responseBody.GetProperty("isSuccess").GetBoolean();
        isSuccess.Should().BeTrue("Response should indicate success");

        // Extract and assert the 'result' property (list of reviews)
        var reviews = responseBody.GetProperty("result").EnumerateArray().ToList();
        reviews.Count.Should().BeGreaterThan(0, "There should be at least one review for the restaurant");
    }

}
