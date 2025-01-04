using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MTOGO.Services.ShoppingCartAPI.Models;
using MTOGO.Services.ShoppingCartAPI.Models.Dto;
using StackExchange.Redis;

namespace MTOGO.IntegrationTests.ShoppingCart
{
    public class ShoppingCartIntegrationTests : IClassFixture<CustomShoppingCartWebApplicationFactory<MTOGO.Services.ShoppingCartAPI.Program>>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;
        private readonly string _cartKey = "cart:TestUser";

        public ShoppingCartIntegrationTests(CustomShoppingCartWebApplicationFactory<MTOGO.Services.ShoppingCartAPI.Program> factory)
        {
            _client = factory.CreateClient();
            _redis = factory.Services.GetRequiredService<IConnectionMultiplexer>();
            _db = _redis.GetDatabase();
        }

        public async Task InitializeAsync()
        {
            await _db.KeyDeleteAsync(_cartKey);
        }

        public async Task DisposeAsync()
        {
            await _db.KeyDeleteAsync(_cartKey);
        }

        [Fact]
        public async Task GetCart_ShouldReturnEmptyCart_WhenNoItemsExist()
        {
            var clean = await _client.DeleteAsync("api/shoppingcart/DeleteCartBy/TestUser");
            var response = await _client.GetAsync("api/shoppingcart/GetCartBy/TestUser");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();

            var cart = JsonSerializer.Deserialize<Cart>(
                responseData.Result!.ToString(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            cart.Should().NotBeNull();
            cart!.UserId.Should().Be("TestUser");
            cart.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task SetCart_ShouldUpdateCart()
        {
            var cart = new Cart
            {
                UserId = "TestUser",
                Items = new List<CartItem>
                {
                    new CartItem { MenuItemId = 1, Quantity = 2, Price = 10.00m },
                    new CartItem { MenuItemId = 2, Quantity = 1, Price = 20.00m }
                }
            };

            var response = await _client.PostAsJsonAsync("api/shoppingcart/SetCart", cart);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();

            var updatedCart = JsonSerializer.Deserialize<Cart>(
                responseData.Result!.ToString(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            updatedCart.Should().NotBeNull();
            updatedCart!.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task CreateCart_ShouldReturnCreated()
        {
            var cart = new Cart
            {
                UserId = "TestUser",
                Items = new List<CartItem>
                {
                    new CartItem { RestaurantId = 1, MenuItemId = 1, Quantity = 2, Price = 10.00m }
                }
            };

            var response = await _client.PostAsJsonAsync("api/shoppingcart/CreateCart", cart);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();

            var createdCart = JsonSerializer.Deserialize<Cart>(
                responseData.Result!.ToString(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            createdCart.Should().NotBeNull();
            createdCart!.UserId.Should().Be("TestUser");
            createdCart.Items.Should().HaveCount(1);
        }

        [Fact]
        public async Task RemoveCart_ShouldReturnOk()
        {
            await _db.StringSetAsync(_cartKey, "{\"UserId\":\"TestUser\",\"Items\":[{\"MenuItemId\":1,\"Quantity\":2,\"Price\":10.00}]}");

            var response = await _client.DeleteAsync("api/shoppingcart/DeleteCartBy/TestUser");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();

        }
    }
}
