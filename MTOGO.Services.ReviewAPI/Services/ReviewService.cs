using MTOGO.Services.ReviewAPI.Models;
using MTOGO.Services.ReviewAPI.Models.Dto;
using MTOGO.Services.ReviewAPI.Services.IServices;
using MTOGO.Services.DataAccess;
using Microsoft.Extensions.Logging;

namespace MTOGO.Services.ReviewAPI.Services {
    public class ReviewService : IReviewService {
        private readonly IDataAccess _dataAccess;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(IDataAccess dataAccess, ILogger<ReviewService> logger) {
            _dataAccess = dataAccess;
            _logger = logger;
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
                _logger.LogError(ex, "Error adding restaurant review.");
                throw;
            }
        }

        public async Task<bool> DeleteRestaurantReviewAsync(int id) {
            try {
                var sql = "DELETE FROM RestaurantReview WHERE Id = @Id;";
                var rowsAffected = await _dataAccess.Delete(sql, new { Id = id });
                return rowsAffected > 0;
            } catch (Exception ex) {
                _logger.LogError(ex, $"Error deleting restaurant review with ID {id}.");
                throw;
            }
        }

        public async Task<RestaurantReview?> GetRestaurantReviewAsync(int id) {
            var sql = "SELECT * FROM RestaurantReview WHERE Id = @Id;";
            return await _dataAccess.GetById<RestaurantReview>(sql, new { Id = id });
        }
    }
}
