using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MTOGO.MessageBus;
using MTOGO.Services.ShoppingCartAPI.Models;
using MTOGO.Services.ShoppingCartAPI.Models.Dto;
using MTOGO.Services.ShoppingCartAPI.Services;
using Newtonsoft.Json;

namespace MTOGO.UnitTests.ShoppingCart
{
    public class ShoppingCartUnitTests : IDisposable
    {
        private readonly IDistributedCache _redisCache;
        private readonly Mock<IMessageBus> _messageBusMock;
        private readonly Mock<ILogger<ShoppingCartService>> _loggerMock;
        private readonly ShoppingCartService _shoppingCartService;

        public ShoppingCartUnitTests()
        {
            var serviceProvider = new ServiceCollection()
                .AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = "localhost:6379";
                    options.InstanceName = "ShoppingCartTests_";
                })
                .BuildServiceProvider();

            _redisCache = serviceProvider.GetRequiredService<IDistributedCache>();
            _messageBusMock = new Mock<IMessageBus>();
            _loggerMock = new Mock<ILogger<ShoppingCartService>>();

            _shoppingCartService = new ShoppingCartService(
                _redisCache,
                _messageBusMock.Object,
                _loggerMock.Object
            );
        }

        public void Dispose()
        {
            _redisCache.Remove("user123"); 
        }

        [Fact]
        public async Task GetCart_ReturnsCart_WhenExistsInCache()
        {
            var userId = "user123";
            var cart = new Cart { UserId = userId, Items = new List<CartItem> { new CartItem { MenuItemId = 101, Quantity = 2, Price = 9.99m } } };
            await _redisCache.SetStringAsync(userId, JsonConvert.SerializeObject(cart));

            var result = await _shoppingCartService.GetCart(userId);

            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Single(result.Items);
            Assert.Equal(101, result.Items[0].MenuItemId);
        }

        [Fact]
        public async Task CreateCart_StoresCartAndPublishesMessage()
        {
            var cart = new Cart { UserId = "user123" };

            var result = await _shoppingCartService.CreateCart(cart);

            var storedCartData = await _redisCache.GetStringAsync(cart.UserId);
            Assert.NotNull(storedCartData);

            var storedCart = JsonConvert.DeserializeObject<Cart>(storedCartData);
            Assert.Equal(cart.UserId, storedCart.UserId);
            _messageBusMock.Verify(m => m.PublishMessage("CartCreatedQueue", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCart_UpdatesCartAndPublishesMessage()
        {
            var cart = new Cart { UserId = "user123", Items = new List<CartItem> { new CartItem { MenuItemId = 101, Quantity = 1, Price = 9.99m } } };
            await _redisCache.SetStringAsync(cart.UserId, JsonConvert.SerializeObject(cart));

            cart.Items[0].Quantity = 3;

            var result = await _shoppingCartService.UpdateCart(cart);

            var updatedCartData = await _redisCache.GetStringAsync(cart.UserId);
            Assert.NotNull(updatedCartData);

            var updatedCart = JsonConvert.DeserializeObject<Cart>(updatedCartData);
            Assert.Equal(3, updatedCart.Items[0].Quantity);
            _messageBusMock.Verify(m => m.PublishMessage("CartUpdatedQueue", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RemoveCart_RemovesCartAndPublishesMessage()
        {
            var userId = "user123";
            await _redisCache.SetStringAsync(userId, JsonConvert.SerializeObject(new Cart { UserId = userId }));

            var result = await _shoppingCartService.RemoveCart(userId);

            var removedCartData = await _redisCache.GetStringAsync(userId);
            Assert.Null(removedCartData);
            _messageBusMock.Verify(m => m.PublishMessage("CartRemovedQueue", It.IsAny<string>()), Times.Once);
            Assert.True(result);
        }

        [Fact]
        public async Task ProcessCartRequest_SendsCartResponseMessage()
        {
            var userId = "user123";
            var cartRequest = new CartRequestMessageDto { UserId = userId, CorrelationId = Guid.NewGuid() };
            var cart = new Cart
            {
                UserId = userId,
                Items = new List<CartItem>
            {
                new CartItem { RestaurantId = 1, MenuItemId = 101, Quantity = 2, Price = 9.99m }
            }
            };
            await _redisCache.SetStringAsync(userId, JsonConvert.SerializeObject(cart));

            await _shoppingCartService.ProcessCartRequest(cartRequest);

            _messageBusMock.Verify(m => m.PublishMessage("CartResponseQueue", It.IsAny<string>()), Times.Once);
        }
    }
}