﻿using Microsoft.Extensions.Caching.Distributed;
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
            try
            {
                var cartData = await _redisCache.GetStringAsync(userId);
                return string.IsNullOrEmpty(cartData) ? null : JsonConvert.DeserializeObject<Cart>(cartData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving cart for user {userId}.");
                throw;
            }
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

                // Find the item to remove
                var itemToRemove = cart.Items.FirstOrDefault(item => item.MenuItemId == menuItemId);
                if (itemToRemove == null)
                {
                    _logger.LogWarning($"MenuItemId {menuItemId} not found in cart for user {userId}.");
                    return false;
                }

                // Remove the item
                cart.Items.Remove(itemToRemove);

                // If the cart is now empty, remove the entire cart from Redis
                if (!cart.Items.Any())
                {
                    await _redisCache.RemoveAsync(userId);
                    _logger.LogInformation($"Cart for user {userId} has been removed as it became empty.");
                    return true;
                }

                // Update the cart in Redis
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
