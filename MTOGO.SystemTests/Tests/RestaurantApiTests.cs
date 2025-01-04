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
    public async Task GetAllRestaurants_ShouldReturnSuccessResponse() {
        // Arrange
        var client = new RestClient(_baseApiUrl);
        var request = new RestRequest("api/restaurant/allRestaurants", Method.Get);

        // Act
        var response = await client.ExecuteAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }


}
