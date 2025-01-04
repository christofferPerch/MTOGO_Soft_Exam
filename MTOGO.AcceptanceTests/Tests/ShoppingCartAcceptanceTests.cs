using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using MTOGO.Services.ShoppingCartAPI.Models;
using MTOGO.Services.ShoppingCartAPI.Models.Dto;
using Newtonsoft.Json;

public class ShoppingCartAcceptanceTests : IAsyncLifetime {
    private readonly HttpClient _client;
    private readonly string[] _testUserIds = { "test-user", "test-user-view", "test-user-delete" };

    public ShoppingCartAcceptanceTests() {
        _client = new HttpClient { BaseAddress = new Uri("http://localhost:5001/api/shoppingcart/") }; // Update with your API base URL
    }

    public async Task InitializeAsync() {
        // Setup: Create carts for all test users
        foreach (var userId in _testUserIds) {
            var cart = new {
                UserId = userId,
                Items = new[] {
                    new { RestaurantId = 1, MenuItemId = 1, Quantity = 2, Price = 12.99 }
                }
            };
            var response = await _client.PostAsJsonAsync("CreateCart", cart);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Conflict);
        }
    }

    public async Task DisposeAsync() {
        // Teardown: Delete carts for all test users
        foreach (var userId in _testUserIds) {
            var response = await _client.DeleteAsync($"DeleteCartBy/{userId}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task CustomerCanCreateCart_ShouldSucceed() {
        // Arrange
        var userId = "test-user-create";
        var cart = new {
            UserId = userId,
            Items = new[] {
                new { RestaurantId = 1, MenuItemId = 1, Quantity = 2, Price = 12.99 }
            }
        };

        // Ensure no existing cart for this user
        var deleteResponse = await _client.DeleteAsync($"DeleteCartBy/{userId}");
        deleteResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

        // Act
        var response = await _client.PostAsJsonAsync("CreateCart", cart);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, "Creating a cart should succeed");

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ResponseDto>(responseBody);

        result.Should().NotBeNull("Response should not be null");
        result.IsSuccess.Should().BeTrue("Response should indicate success");

        var createdCart = JsonConvert.DeserializeObject<Cart>(result.Result.ToString());
        createdCart.Should().NotBeNull("Created cart should not be null");
        createdCart.UserId.Should().Be(userId, "The created cart should belong to the specified user");
        createdCart.Items.Should().NotBeEmpty("The created cart should contain items");
    }

    [Fact]
    public async Task CustomerCanViewCart_ShouldReturnCartDetails() {
        // Arrange
        var userId = "test-user-view";

        // Act
        var response = await _client.GetAsync($"GetCartBy/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Retrieving the cart should succeed");

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ResponseDto>(responseBody);

        result.Should().NotBeNull("Response should not be null");
        result.IsSuccess.Should().BeTrue("Response should indicate success");

        var retrievedCart = JsonConvert.DeserializeObject<Cart>(result.Result.ToString());
        retrievedCart.Should().NotBeNull("Retrieved cart should not be null");
        retrievedCart.UserId.Should().Be(userId, "The cart should belong to the specified user");
        retrievedCart.Items.Should().NotBeEmpty("The cart should contain items");
    }

    [Fact]
    public async Task CustomerCanAddItemsToCart_ShouldSucceed() {
        // Arrange
        var cart = new {
            UserId = "test-user",
            Items = new[] {
                new { RestaurantId = 1, MenuItemId = 2, Quantity = 3, Price = 10.99 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("SetCart", cart);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Adding items to the cart should succeed");

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ResponseDto>(responseBody);

        result.Should().NotBeNull("Response should not be null");
        result.IsSuccess.Should().BeTrue("Response should indicate success");
    }

    [Fact]
    public async Task CustomerCanRemoveItemFromCart_ShouldSucceed() {
        // Arrange
        string userId = "test-user";
        int menuItemId = 1; // Assuming an item with this ID exists in the cart

        // Act
        var response = await _client.DeleteAsync($"RemoveFromCart?userId={userId}&menuItemId={menuItemId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Removing an item from the cart should succeed");

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ResponseDto>(responseBody);

        result.Should().NotBeNull("Response should not be null");
        result.IsSuccess.Should().BeTrue("Response should indicate success");
    }

    [Fact]
    public async Task CustomerCanDeleteCart_ShouldSucceed() {
        // Arrange
        var userId = "test-user-delete";

        // Act
        var deleteResponse = await _client.DeleteAsync($"DeleteCartBy/{userId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Deleting the cart should succeed");

        var responseBody = await deleteResponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ResponseDto>(responseBody);

        result.Should().NotBeNull("Response should not be null");
        result.IsSuccess.Should().BeTrue("Response should indicate success");
        result.Message.Should().Be("Cart removed successfully.", "The success message should indicate successful deletion");
    }
}
