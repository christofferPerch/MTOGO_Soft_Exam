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
            var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
            var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";
            var redisConfiguration = $"{redisHost}:{redisPort}";

            var serviceProvider = new ServiceCollection()
                .AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConfiguration;
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

        [Fact]
        public async Task SetCart_UpdatesExistingItems_AddsNewItems_RemovesZeroQuantityItems()
        {
            var initialCart = new Cart
            {
                UserId = _testUserId,
                Items = new List<CartItem>
        {
            new CartItem { MenuItemId = 101, Quantity = 2, Price = 9.99m },
            new CartItem { MenuItemId = 102, Quantity = 3, Price = 19.99m }
        }
            };

            await _redisCache.SetStringAsync(_testUserId, JsonConvert.SerializeObject(initialCart));

            var updatedCart = new Cart
            {
                UserId = _testUserId,
                Items = new List<CartItem>
        {
            new CartItem { MenuItemId = 101, Quantity = 5, Price = 9.99m },  
            new CartItem { MenuItemId = 103, Quantity = 2, Price = 29.99m }, 
            new CartItem { MenuItemId = 102, Quantity = 0 } 
        }
            };

            var result = await _shoppingCartService.SetCart(updatedCart);

            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(5, result.Items.First(i => i.MenuItemId == 101).Quantity);
            Assert.Equal(29.99m, result.Items.First(i => i.MenuItemId == 103).Price);
            Assert.DoesNotContain(result.Items, i => i.MenuItemId == 102);
        }

        [Fact]
        public async Task RemoveCart_NonExistentCart_ShouldReturnFalse()
        {
            var result = await _shoppingCartService.RemoveCart(_testUserId);

            Assert.False(result);
            _loggerMock.Verify(
                log => log.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }


        [Fact]
        public async Task RemoveMenuItem_NonExistentItem_ShouldReturnFalse()
        {
            var initialCart = new Cart
            {
                UserId = _testUserId,
                Items = new List<CartItem>
        {
            new CartItem { MenuItemId = 101, Quantity = 2, Price = 9.99m }
        }
            };

            await _redisCache.SetStringAsync(_testUserId, JsonConvert.SerializeObject(initialCart));

            var result = await _shoppingCartService.RemoveMenuItem(_testUserId, 999);

            Assert.False(result);
            Assert.Single(initialCart.Items); 
        }

        [Fact]
        public async Task CreateCart_CartAlreadyExists_ShouldThrowException()
        {
            var existingCart = new Cart { UserId = _testUserId };
            await _redisCache.SetStringAsync(_testUserId, JsonConvert.SerializeObject(existingCart));

            var newCart = new Cart { UserId = _testUserId };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _shoppingCartService.CreateCart(newCart));
        }

        [Fact]
        public async Task ProcessCartRequest_NonExistentCart_ShouldNotPublishMessage()
        {
            var cartRequest = new CartRequestMessageDto
            {
                UserId = _testUserId,
                CorrelationId = Guid.NewGuid()
            };

            await _shoppingCartService.ProcessCartRequest(cartRequest);

            _messageBusMock.Verify(m => m.PublishMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SetCart_NullCart_ShouldThrowArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _shoppingCartService.SetCart(null));
        }


        [Fact]
        public async Task SetCart_EmptyUserId_ShouldThrowArgumentException()
        {
            var invalidCart = new Cart { UserId = "" };

            await Assert.ThrowsAsync<ArgumentException>(() => _shoppingCartService.SetCart(invalidCart));
        }


    }
}
