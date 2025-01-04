using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using MTOGO.Services.ShoppingCartAPI.Models.Dto;
using System.Text.Json;


public class ShoppingCartAPITests {
    private readonly HttpClient _client;

    public ShoppingCartAPITests() {
        _client = new HttpClient { BaseAddress = new Uri("http://localhost:5001") }; // Replace with Gateway/Shopping Cart API URL
    }

    [Fact]
    public async Task AddItemsToCart_ShouldReturnSuccess() {
        var cart = new {
            userId = "test-user",
            items = new[]
            {
                new { restaurantId = 1, menuItemId = 1, quantity = 2, price = 12.99 }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/shoppingcart/SetCart", cart);

        response.StatusCode.Should().Be(HttpStatusCode.OK, "Adding items to the cart should succeed");
    }

    [Fact]
    public async Task RemoveItemFromCart_ShouldReturnSuccess() {
        string userId = "test-user";
        int menuItemId = 1; // Assuming an item with this ID exists in the cart
        var response = await _client.DeleteAsync($"/api/shoppingcart/RemoveFromCart?userId={userId}&menuItemId={menuItemId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK, "Removing an item from the cart should succeed");
    }

    [Fact]
    public async Task DeleteCart_ShouldReturnSuccess() {
        // Step 1: Ensure the cart exists
        var cart = new {
            userId = "test-user-delete",
            items = new[]
            {
            new { restaurantId = 1, menuItemId = 1, quantity = 2, price = 12.99 }
        }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/shoppingcart/SetCart", cart);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Creating a cart is required before deletion");

        // Step 2: Delete the cart
        var response = await _client.DeleteAsync($"/api/shoppingcart/DeleteCartBy/{cart.userId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Deleting the cart should succeed");

        // Step 3: Deserialize and validate response
        string responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ResponseDto>(responseContent);

        result.Should().NotBeNull("Response should not be null");
        result.IsSuccess.Should().BeTrue("Response should indicate success");
    }


}
