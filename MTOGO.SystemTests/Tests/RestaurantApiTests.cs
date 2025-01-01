using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using RestSharp;
using Newtonsoft.Json.Linq; // For JSON parsing
using Xunit;
using Newtonsoft.Json;

public class RestaurantApiTests {
    private readonly string _baseApiUrl;

    public RestaurantApiTests() {
        var configuration = ConfigurationHelper.LoadConfiguration();
        _baseApiUrl = configuration["RestaurantApiBaseUrl"];
    }

    [Fact]
    public async Task GetAllRestaurants_ShouldReturnOk() {
        // Arrange
        var client = new RestClient(_baseApiUrl);
        var request = new RestRequest("api/restaurant/allRestaurants", Method.Get);

        // Act
        var response = await client.ExecuteAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllRestaurants_ShouldReturnListOfRestaurants() {
        // Arrange
        var client = new RestClient(_baseApiUrl);
        var request = new RestRequest("api/restaurant/allRestaurants", Method.Get);

        // Act
        var response = await client.ExecuteAsync(request);

        // Log the response for debugging
        Console.WriteLine(response.Content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Deserialize the response
        var apiResponse = JsonConvert.DeserializeObject<dynamic>(response.Content);

        // Extract the result list from the response
        var restaurants = apiResponse.result as IEnumerable<dynamic>;

        // Ensure the list is not empty
        restaurants.Should().NotBeNullOrEmpty("Expected at least one restaurant to be returned");

        // Ensure the first restaurant has a valid name
        string restaurantName = restaurants.First().restaurantName.ToString();
        restaurantName.Should().NotBeNullOrEmpty("Expected 'restaurantName' field in the first restaurant");
    }

}
