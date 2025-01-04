using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MTOGO.Services.ShoppingCartAPI.Models.Dto;


public class ShoppingCartAPITests
{
    private readonly HttpClient _client;

    public ShoppingCartAPITests()
    {
        _client = new HttpClient { BaseAddress = new Uri("http://localhost:5001") };
    }

    [Fact]
    public async Task AddItemsToCart_ShouldReturnSuccess()
    {
        var cart = new
        {
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
    public async Task DeleteCart_ShouldReturnSuccess()
    {
        var cart = new
        {
            userId = "test-user-delete",
            items = new[]
            {
            new { restaurantId = 1, menuItemId = 1, quantity = 2, price = 12.99 }
        }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/shoppingcart/SetCart", cart);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Creating a cart is required before deletion");

        var response = await _client.DeleteAsync($"/api/shoppingcart/DeleteCartBy/{cart.userId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Deleting the cart should succeed");

        string responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ResponseDto>(responseContent);

        result.Should().NotBeNull("Response should not be null");
        result.IsSuccess.Should().BeTrue("Response should indicate success");
    }
}
