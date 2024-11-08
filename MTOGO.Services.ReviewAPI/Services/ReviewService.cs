using MTOGO.Services.ReviewAPI.Models;
using MTOGO.Services.ReviewAPI.Models.Dto;
using MTOGO.Services.ReviewAPI.Services.IServices;
using MTOGO.Services.DataAccess;
using Microsoft.Extensions.Logging;
using MTOGO.MessageBus;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using MTOGO.Services.OrderAPI.Models.Dto;
using MTOGO.Services.OrderAPI.Models;


namespace MTOGO.Services.ReviewAPI.Services {
    public class ReviewService : IReviewService {
        private readonly IDataAccess _dataAccess;
        private readonly ILogger<ReviewService> _logger;
        private readonly IMessageBus _messageBus;
        private readonly IDistributedCache _distributedCache;
        private const string OrderStatusQueue = "OrderStatusQueue";

        public ReviewService(IDataAccess dataAccess, IMessageBus messageBus, IDistributedCache distributedCache, ILogger<ReviewService> logger) {
            _dataAccess = dataAccess;
            _messageBus = messageBus;
            _distributedCache = distributedCache;
            _logger = logger;

            SubscribeToOrderStatusQueue();
        }

        private void SubscribeToOrderStatusQueue() {
            _messageBus.SubscribeMessage<OrderStatusUpdateDto>(OrderStatusQueue, async (statusUpdate) => {
                _logger.LogInformation($"Received order status update for OrderId {statusUpdate.OrderId}, StatusId: {statusUpdate.StatusId}");

                var cacheKey = GetOrderStatusCacheKey(statusUpdate.OrderId);
                await _distributedCache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(statusUpdate.StatusId), new DistributedCacheEntryOptions {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) 
                });
            });
        }

        public async Task<int> AddDeliveryReviewAsync(DeliveryReviewDto deliveryReviewDto) {
            try {
                var cacheKey = GetOrderStatusCacheKey(deliveryReviewDto.OrderId);
                var orderStatusJson = await _distributedCache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(orderStatusJson) || JsonConvert.DeserializeObject<int>(orderStatusJson) != (int)OrderStatus.Delivered) {
                    throw new InvalidOperationException("Cannot submit Review for unconfirmed orders.");
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
