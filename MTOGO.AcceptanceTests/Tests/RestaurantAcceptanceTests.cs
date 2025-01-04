using System.Net;
using System.Net.Http;
using System.Text;
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
        var response = await _client.GetAsync("/restaurant/allRestaurants");
        response.StatusCode.Should().Be(HttpStatusCode.OK, "The endpoint should return menus with details");

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(responseBody);

        // Validate response structure and data
        Assert.NotNull(result?.result);
        Assert.IsType<JArray>(result.result);

        var restaurants = (JArray)result.result;
        Assert.NotEmpty(restaurants);

        var firstRestaurant = restaurants.FirstOrDefault();
        Assert.NotNull(firstRestaurant);
        Assert.False(string.IsNullOrEmpty((string)firstRestaurant["restaurantName"]), "Restaurant name should not be null or empty");
        Assert.True(firstRestaurant["menuItems"].HasValues, "Menu items should not be null or empty");
    }


    [Fact]
    public async Task RestaurantOwnerCanAddMenuItem_ShouldReturnSuccess() {
        // Arrange
        var menuItem = new {
            restaurantId = 1, // Assuming this restaurant exists in the system.
            name = "New Dish",
            description = "A delicious new dish.",
            price = 15.99
        };

        // Act
        var response = await _client.PostAsync(
            "/restaurant/addMenuItem",
            new StringContent(JsonConvert.SerializeObject(menuItem), Encoding.UTF8, "application/json")
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK, "The endpoint should allow adding a new menu item");

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(responseBody);

        // Validate response structure and data
        Assert.True((bool)result.isSuccess, "Adding menu item should be successful");
        Assert.NotNull(result.message);
    }


}
