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
        private readonly string _testUserId;

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

            _testUserId = $"user_{Guid.NewGuid()}";
        }

        public void Dispose()
        {
            _redisCache.Remove(_testUserId);
        }

        [Fact]
        public async Task GetCart_ReturnsCart_WhenExistsInCache()
        {
            var cart = new Cart { UserId = _testUserId, Items = new List<CartItem> { new CartItem { MenuItemId = 101, Quantity = 2, Price = 9.99m } } };
            await _redisCache.SetStringAsync(_testUserId, JsonConvert.SerializeObject(cart));

            var result = await _shoppingCartService.GetCart(_testUserId);

            Assert.NotNull(result);
            Assert.Equal(_testUserId, result.UserId);
            Assert.Single(result.Items);
            Assert.Equal(101, result.Items[0].MenuItemId);
        }

        [Fact]
        public async Task CreateCart_StoresCartAndPublishesMessage()
        {
            var cart = new Cart { UserId = _testUserId };

            var result = await _shoppingCartService.CreateCart(cart);

            var storedCartData = await _redisCache.GetStringAsync(_testUserId);
            Assert.NotNull(storedCartData);

            var storedCart = JsonConvert.DeserializeObject<Cart>(storedCartData);
            Assert.Equal(cart.UserId, storedCart.UserId);
        }

        [Fact]
        public async Task UpdateCart()
        {
            var cart = new Cart { UserId = _testUserId, Items = new List<CartItem> { new CartItem { MenuItemId = 101, Quantity = 1, Price = 9.99m } } };
            await _redisCache.SetStringAsync(_testUserId, JsonConvert.SerializeObject(cart));

            cart.Items[0].Quantity = 3;

            var result = await _shoppingCartService.UpdateCart(cart);

            var updatedCartData = await _redisCache.GetStringAsync(_testUserId);
            Assert.NotNull(updatedCartData);

            var updatedCart = JsonConvert.DeserializeObject<Cart>(updatedCartData);
            Assert.Equal(3, updatedCart.Items[0].Quantity);
        }

        [Fact]
        public async Task RemoveCart()
        {
            await _redisCache.SetStringAsync(_testUserId, JsonConvert.SerializeObject(new Cart { UserId = _testUserId }));

            var result = await _shoppingCartService.RemoveCart(_testUserId);

            var removedCartData = await _redisCache.GetStringAsync(_testUserId);
            Assert.Null(removedCartData);
            Assert.True(result);

            _messageBusMock.Verify(m => m.PublishMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcessCartRequest_SendsCartResponseMessage()
        {
            var cartRequest = new CartRequestMessageDto { UserId = _testUserId, CorrelationId = Guid.NewGuid() };
            var cart = new Cart
            {
                UserId = _testUserId,
                Items = new List<CartItem>
                {
                    new CartItem { RestaurantId = 1, MenuItemId = 101, Quantity = 2, Price = 9.99m }
                }
            };
            await _redisCache.SetStringAsync(_testUserId, JsonConvert.SerializeObject(cart));

            await _shoppingCartService.ProcessCartRequest(cartRequest);

            _messageBusMock.Verify(m => m.PublishMessage("CartResponseQueue", It.IsAny<string>()), Times.Once);
        }
    }
}
