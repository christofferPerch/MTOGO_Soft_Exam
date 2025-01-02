using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

public class RestaurantAcceptanceTests {
    private readonly HttpClient _client;

    public RestaurantAcceptanceTests() {
        _client = new HttpClient { BaseAddress = new Uri("http://localhost:7777") }; // Gateway URL
    }

    [Fact]
    public async Task CustomerCanViewMenus_ShouldReturnMenusWithDetails() {
        // Arrange
        var client = new HttpClient { BaseAddress = new Uri("http://localhost:7777") };

        // Act
        var response = await client.GetAsync("/restaurant/allRestaurants");
        Assert.True(response.IsSuccessStatusCode, $"Expected status code 200, but got {response.StatusCode}");

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(responseBody);

        // Ensure result is not null or empty
        Assert.NotNull(result?.result);
        Assert.IsType<JArray>(result.result);

        var restaurants = (JArray)result.result;
        Assert.NotEmpty(restaurants);

        // Validate the first restaurant's menu details (if any)
        var firstRestaurant = restaurants.FirstOrDefault();
        Assert.NotNull(firstRestaurant);
        Assert.False(string.IsNullOrEmpty((string)firstRestaurant["restaurantName"]), "Restaurant name should not be null or empty");
        Assert.True(firstRestaurant["menuItems"].HasValues, "Menu items should not be null or empty");
    }


}
