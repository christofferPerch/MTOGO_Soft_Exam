using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using MTOGO.MessageBus;
using MTOGO.Services.ShoppingCartAPI.Models;
using MTOGO.Services.ShoppingCartAPI.Models.Dto;
using MTOGO.Services.ShoppingCartAPI.Services.IServices;
using Newtonsoft.Json;

namespace MTOGO.Services.ShoppingCartAPI.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IDistributedCache _redisCache;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<ShoppingCartService> _logger;

        public ShoppingCartService(IDistributedCache redisCache, IMessageBus messageBus, ILogger<ShoppingCartService> logger)
        {
            _redisCache = redisCache;
            _messageBus = messageBus;
            _logger = logger;

            try
            {
                SubscribeToCartRequestQueue();
                SubscribeToCartRemovedQueue();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to RabbitMQ queues.");
                throw;
            }
        }

        private void SubscribeToCartRequestQueue()
        {
            try
            {
                _messageBus.SubscribeMessage<CartRequestMessageDto>("CartRequestQueue", async (cartRequest) =>
                {
                    await ProcessCartRequest(cartRequest);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to CartRequestQueue.");
                throw;
            }
        }

        private void SubscribeToCartRemovedQueue()
        {
            try
            {
                _messageBus.SubscribeMessage<CartRemovedMessageDto>("CartRemovedQueue", async cartRemovedMessage =>
                {
                    if (cartRemovedMessage == null || string.IsNullOrEmpty(cartRemovedMessage.UserId))
                    {
                        _logger.LogWarning("Received an invalid cart removal message.");
                        return;
                    }

                    await RemoveCart(cartRemovedMessage.UserId);
                    _logger.LogInformation($"Cart removed successfully for user: {cartRemovedMessage.UserId}");
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to CartRemovedQueue.");
                throw;
            }
        }


        public async Task<Cart?> GetCart(string userId)
        {
            var serializedCart = await _redisCache.GetStringAsync(userId);

            if (string.IsNullOrEmpty(serializedCart))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<Cart>(serializedCart);
        }



        public async Task<Cart?> CreateCart(Cart cart)
        {
            try
            {
                var existingCart = await GetCart(cart.UserId);
                if (existingCart != null)
                {
                    throw new InvalidOperationException($"Cart already exists for user {cart.UserId}");
                }

                await _redisCache.SetStringAsync(cart.UserId, JsonConvert.SerializeObject(cart));

                return cart;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating cart for user {cart.UserId}.");
                throw;
            }
        }

        public async Task<Cart> UpdateCart(Cart cart)
        {
            try
            {
                await _redisCache.SetStringAsync(cart.UserId, JsonConvert.SerializeObject(cart));


                return await GetCart(cart.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating cart for user {cart.UserId}.");
                throw;
            }
        }

        public async Task<bool> RemoveCart(string userId)
        {
            try
            {
                await _redisCache.RemoveAsync(userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing cart for user {userId}.");
                throw;
            }
        }

        public async Task<Cart?> SetCart(Cart cart)
        {
            try
            {
                if (cart == null || string.IsNullOrEmpty(cart.UserId))
                {
                    throw new ArgumentException("Invalid cart or user ID.");
                }

                var existingCartData = await _redisCache.GetStringAsync(cart.UserId);
                Cart existingCart = existingCartData != null
                    ? JsonConvert.DeserializeObject<Cart>(existingCartData)
                    : new Cart { UserId = cart.UserId };

                foreach (var newItem in cart.Items)
                {
                    var existingItem = existingCart.Items.FirstOrDefault(i => i.MenuItemId == newItem.MenuItemId);
                    if (existingItem != null)
                    {
                        existingItem.Quantity = newItem.Quantity;

                        if (existingItem.Quantity <= 0)
                        {
                            existingCart.Items.Remove(existingItem);
                        }
                    }
                    else if (newItem.Quantity > 0)
                    {
                        existingCart.Items.Add(newItem);
                    }
                }

                if (!existingCart.Items.Any())
                {
                    await _redisCache.RemoveAsync(cart.UserId);
                    _logger.LogInformation($"Cart for user {cart.UserId} has been removed as it is empty.");
                    return null;
                }

                var serializedCart = JsonConvert.SerializeObject(existingCart);
                await _redisCache.SetStringAsync(cart.UserId, serializedCart);

                _logger.LogInformation($"Cart for user {cart.UserId} has been successfully updated.");
                return existingCart;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting cart for user {cart.UserId}.");
                throw;
            }
        }

        public async Task ProcessCartRequest(CartRequestMessageDto cartRequest)
        {
            try
            {
                var cart = await GetCart(cartRequest.UserId);
                if (cart == null)
                {
                    return;
                }

                var cartResponse = new CartResponseMessageDto
                {
                    UserId = cartRequest.UserId,
                    CorrelationId = cartRequest.CorrelationId,
                    Items = cart.Items.Select(item => new OrderItemDto
                    {
                        RestaurantId = item.RestaurantId,
                        MenuItemId = item.MenuItemId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    }).ToList()
                };

                await _messageBus.PublishMessage("CartResponseQueue", JsonConvert.SerializeObject(cartResponse));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing cart request for user {cartRequest.UserId}.");
                throw;
            }
        }

        public async Task<bool> RemoveMenuItem(string userId, int menuItemId)
        {
            try
            {
                var cart = await GetCart(userId);
                if (cart == null)
                {
                    _logger.LogWarning($"Cart not found for user {userId}.");
                    return false;
                }

                var itemToRemove = cart.Items.FirstOrDefault(item => item.MenuItemId == menuItemId);
                if (itemToRemove == null)
                {
                    _logger.LogWarning($"MenuItemId {menuItemId} not found in cart for user {userId}.");
                    return false;
                }

                cart.Items.Remove(itemToRemove);

                if (!cart.Items.Any())
                {
                    await _redisCache.RemoveAsync(userId);
                    _logger.LogInformation($"Cart for user {userId} has been removed as it became empty.");
                    return true;
                }

                await _redisCache.SetStringAsync(userId, JsonConvert.SerializeObject(cart));
                _logger.LogInformation($"MenuItemId {menuItemId} removed from cart for user {userId}, and Redis updated.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing MenuItemId {menuItemId} from cart for user {userId}.");
                throw;
            }
        }
    }
}
