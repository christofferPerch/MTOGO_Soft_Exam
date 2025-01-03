using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MTOGO.Services.ShoppingCartAPI.Models;
using MTOGO.Services.ShoppingCartAPI.Models.Dto;
using StackExchange.Redis;

namespace MTOGO.IntegrationTests.ShoppingCart
{
    public class ShoppingCartIntegrationTests : IClassFixture<CustomShoppingCartWebApplicationFactory<MTOGO.Services.ShoppingCartAPI.Program>>
    {
        private readonly HttpClient _client;
        private readonly IConnectionMultiplexer _redis;

        public ShoppingCartIntegrationTests(CustomShoppingCartWebApplicationFactory<MTOGO.Services.ShoppingCartAPI.Program> factory)
        {
            _client = factory.CreateClient();
            _redis = factory.Services.GetRequiredService<IConnectionMultiplexer>();
        }

        [Fact]
        public async Task GetCart_ShouldReturnEmptyCart_WhenNoItemsExist()
        {
            // Act
            var response = await _client.GetAsync("api/shoppingcart/GetCartBy/TestUser");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();

            var cart = responseData.Result as Cart;
            cart.Should().NotBeNull();
            cart!.UserId.Should().Be("TestUser");
            cart.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task SetCart_ShouldUpdateCart()
        {
            // Arrange
            var cart = new Cart
            {
                UserId = "TestUser",
                Items = new List<CartItem>
                {
                    new CartItem { MenuItemId = 1, Quantity = 2, Price = 10.00m },
                    new CartItem { MenuItemId = 2, Quantity = 1, Price = 20.00m }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/shoppingcart/SetCart", cart);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();

            var updatedCart = responseData.Result as Cart;
            updatedCart.Should().NotBeNull();
            updatedCart!.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task CreateCart_ShouldReturnCreated()
        {
            // Arrange
            var cart = new Cart
            {
                UserId = "TestUser",
                Items = new List<CartItem>
                {
                    new CartItem { RestaurantId = 1, MenuItemId = 1, Quantity = 2, Price = 10.00m }
                }
            };


            // Act
            var response = await _client.PostAsJsonAsync("api/shoppingcart/CreateCart", cart);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();

            var createdCart = responseData.Result as Cart;
            createdCart.Should().NotBeNull();
            createdCart!.UserId.Should().Be("TestUser");
            createdCart.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task RemoveMenuItem_ShouldReturnOk()
        {
            // Arrange: Set a cart with items
            var db = _redis.GetDatabase();
            var cartKey = $"cart:TestUser";
            await db.StringSetAsync(cartKey, "{\"UserId\":\"TestUser\",\"Items\":[{\"MenuItemId\":1,\"Quantity\":2,\"Price\":10.00}]}");

            // Act: Remove an item from the cart
            var response = await _client.DeleteAsync("api/shoppingcart/RemoveFromCart?userId=TestUser&menuItemId=1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();

            var updatedCart = responseData.Result as Cart;
            updatedCart.Should().NotBeNull();
            updatedCart!.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task RemoveCart_ShouldReturnOk()
        {
            // Arrange: Create a cart in Redis
            var db = _redis.GetDatabase();
            var cartKey = $"cart:TestUser";
            await db.StringSetAsync(cartKey, "{\"UserId\":\"TestUser\",\"Items\":[{\"MenuItemId\":1,\"Quantity\":2,\"Price\":10.00}]}");

            // Act: Remove the cart
            var response = await _client.DeleteAsync("api/shoppingcart/DeleteCartBy/TestUser");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();

            var cartExists = await db.KeyExistsAsync(cartKey);
            cartExists.Should().BeFalse();
        }
    }
}
