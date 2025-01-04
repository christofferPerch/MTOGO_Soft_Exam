using System.Net;
using FluentAssertions;
using RestSharp;

public class RestaurantApiTests
{
    private readonly string _baseApiUrl;

    public RestaurantApiTests()
    {
        var configuration = ConfigurationHelper.LoadConfiguration();
        _baseApiUrl = configuration["RestaurantApiBaseUrl"];
    }

    [Fact]
    public async Task GetAllRestaurants_ShouldReturnOk()
    {
        var client = new RestClient(_baseApiUrl);
        var request = new RestRequest("api/restaurant/allRestaurants", Method.Get);

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllRestaurants_ShouldReturnSuccessResponse()
    {
        var client = new RestClient(_baseApiUrl);
        var request = new RestRequest("api/restaurant/allRestaurants", Method.Get);

        var response = await client.ExecuteAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
