using MTOGO.Services.DataAccess;
using MTOGO.Services.ReviewAPI.Models;
using MTOGO.Services.ReviewAPI.Models.Dto;
using MTOGO.Services.ReviewAPI.Services.IServices;

namespace MTOGO.Services.ReviewAPI.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IDataAccess _dataAccess;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(IDataAccess dataAccess, ILogger<ReviewService> logger)
        {
            _dataAccess = dataAccess;
            _logger = logger;
        }

        public async Task<int> AddRestaurantReviewAsync(RestaurantReviewDto restaurantReviewDto)
        {
            try
            {
                if (restaurantReviewDto == null || restaurantReviewDto.RestaurantId <= 0)
                {
                    throw new ArgumentException("Invalid review data. RestaurantId must be provided.");
                }

                var sql = @"
                        INSERT INTO RestaurantReview (CustomerId, RestaurantId, FoodRating, Comments, ReviewTimestamp)
                        VALUES (@CustomerId, @RestaurantId, @FoodRating, @Comments, @ReviewTimestamp);
                        SELECT CAST(SCOPE_IDENTITY() as int);";

                var parameters = new
                {
                    restaurantReviewDto.CustomerId,
                    restaurantReviewDto.RestaurantId,
                    restaurantReviewDto.FoodRating,
                    restaurantReviewDto.Comments,
                    ReviewTimestamp = DateTime.Now
                };

                var id = await _dataAccess.InsertAndGetId<int>(sql, parameters);
                return id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding restaurant review.");
                throw;
            }
        }


        public async Task<List<RestaurantReview>?> GetRestaurantReviewAsync(int restaurantId)
        {
            try
            {
                var sql = "SELECT * FROM RestaurantReview WHERE RestaurantId = @RestaurantId;";
                return await _dataAccess.GetAll<RestaurantReview>(sql, new { RestaurantId = restaurantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving reviews for RestaurantId {restaurantId}.");
                throw;
            }
        }


        public async Task<bool> DeleteRestaurantReviewAsync(int id)
        {
            try
            {
                var sql = "DELETE FROM RestaurantReview WHERE Id = @Id;";
                var rowsAffected = await _dataAccess.Delete(sql, new { Id = id });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting restaurant review with ID {id}.");
                throw;
            }
        }
    }
}
