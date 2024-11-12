using MTOGO.Services.ReviewAPI.Models;
using MTOGO.Services.ReviewAPI.Models.Dto;
using MTOGO.Services.ReviewAPI.Services.IServices;
using MTOGO.Services.DataAccess;
using Microsoft.Extensions.Logging;
using MTOGO.MessageBus;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace MTOGO.Services.ReviewAPI.Services {
    public class ReviewService : IReviewService {
        private readonly IDataAccess _dataAccess;
        private readonly ILogger<ReviewService> _logger;
        private readonly IMessageBus _messageBus;
        private readonly IDistributedCache _distributedCache;
        private const string OrderStatusQueue = "OrderStatusQueue";
        private const string OrderStatusRequestQueue = "OrderStatusRequestQueue";
        private const string OrderStatusResponseQueue = "OrderStatusResponseQueue";

        public ReviewService(IDataAccess dataAccess, IMessageBus messageBus, IDistributedCache distributedCache, ILogger<ReviewService> logger) {
            _dataAccess = dataAccess;
            _messageBus = messageBus;
            _distributedCache = distributedCache;
            _logger = logger;

            SubscribeToOrderStatusQueue();
        }

        private void SubscribeToOrderStatusQueue() {
            _messageBus.SubscribeMessage<OrderStatusResponseDto>(OrderStatusQueue, async (statusUpdate) =>
            {
                if (statusUpdate != null) {
                    _logger.LogInformation($"Received order status update for OrderId {statusUpdate.OrderId}, StatusId: {statusUpdate.StatusId}");

                    var cacheKey = GetOrderStatusCacheKey(statusUpdate.OrderId);
                    await _distributedCache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(statusUpdate.StatusId), new DistributedCacheEntryOptions {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                    });
                } else {
                    _logger.LogWarning("Received a null order status update message.");
                }
            });
        }


        public async Task<int> AddDeliveryReviewAsync(DeliveryReviewDto deliveryReviewDto) {
            try {
                // Fetch or request Order Status from OrderService
                var orderStatus = await GetOrderStatus(deliveryReviewDto.OrderId);

                if (orderStatus != (int)OrderStatus.Delivered) {
                    throw new InvalidOperationException("Cannot submit Review for orders that are not delivered.");
                }

                var sql = @"
                    INSERT INTO DeliveryReview (OrderId, CustomerId, DeliveryAgentId, DeliveryExperienceRating, Comments, ReviewTimestamp)
                    VALUES (@OrderId, @CustomerId, @DeliveryAgentId, @DeliveryExperienceRating, @Comments, @ReviewTimestamp);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new {
                    deliveryReviewDto.OrderId,
                    deliveryReviewDto.CustomerId,
                    deliveryReviewDto.DeliveryAgentId,
                    deliveryReviewDto.DeliveryExperienceRating,
                    deliveryReviewDto.Comments,
                    ReviewTimestamp = DateTime.Now
                };

                var id = await _dataAccess.InsertAndGetId<int>(sql, parameters);
                return id;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error adding delivery Review.");
                throw;
            }
        }

        private async Task<int> GetOrderStatus(int orderId) {
            var cacheKey = GetOrderStatusCacheKey(orderId);
            var orderStatusJson = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(orderStatusJson)) {
                return JsonConvert.DeserializeObject<int>(orderStatusJson);
            }

            var orderStatusRequest = new OrderStatusRequestDto { OrderId = orderId };
            var tcs = new TaskCompletionSource<int>();

            _messageBus.PublishMessage(OrderStatusRequestQueue, JsonConvert.SerializeObject(orderStatusRequest));

            _messageBus.SubscribeMessage<OrderStatusResponseDto>(OrderStatusResponseQueue, response => {
                if (response.OrderId == orderId) {
                    tcs.SetResult(response.StatusId);
                }
            });

            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(30)));
            if (completedTask == tcs.Task) {
                int statusId = tcs.Task.Result;
                await _distributedCache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(statusId), new DistributedCacheEntryOptions {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                });
                return statusId;
            }

            throw new TimeoutException("Failed to retrieve order status from OrderService.");
        }

        public async Task<int> AddRestaurantReviewAsync(RestaurantReviewDto restaurantReviewDto) {
            try {
                var sql = @"
                    INSERT INTO RestaurantReview (CustomerId, FoodRating, Comments, ReviewTimestamp)
                    VALUES (@CustomerId, @FoodRating, @Comments, @ReviewTimestamp);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new {
                    restaurantReviewDto.CustomerId,
                    restaurantReviewDto.FoodRating,
                    restaurantReviewDto.Comments,
                    ReviewTimestamp = DateTime.Now
                };

                var id = await _dataAccess.InsertAndGetId<int>(sql, parameters);
                return id;
            } catch (Exception ex) {
                _logger.LogError(ex, "Error adding restaurant Review.");
                throw;
            }
        }

        public async Task<DeliveryReview?> GetDeliveryReviewAsync(int id) {
            var sql = "SELECT * FROM DeliveryReview WHERE Id = @Id;";
            return await _dataAccess.GetById<DeliveryReview>(sql, new { Id = id });
        }

        public async Task<RestaurantReview?> GetRestaurantReviewAsync(int id) {
            var sql = "SELECT * FROM RestaurantReview WHERE Id = @Id;";
            return await _dataAccess.GetById<RestaurantReview>(sql, new { Id = id });
        }

        private static string GetOrderStatusCacheKey(int orderId) => $"order_status:{orderId}";
    }
}
