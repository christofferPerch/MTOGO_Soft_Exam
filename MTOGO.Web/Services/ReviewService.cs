using MTOGO.Web.Models;
using MTOGO.Web.Models.Review;
using MTOGO.Web.Services.IServices;
using MTOGO.Web.Utility;
using System.Threading.Tasks;

namespace MTOGO.Web.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IBaseService _baseService;

        public ReviewService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> AddRestaurantReviewAsync(RestaurantReviewDto restaurantReviewDto)
        {
            try
            {
                return await _baseService.SendAsync(new RequestDto
                {
                    ApiType = SD.ApiType.POST,
                    Url = $"{SD.ReviewAPIBase}/api/review/restaurant/add",
                    Data = restaurantReviewDto
                });
            }
            catch (Exception ex)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = $"An error occurred while sending the review: {ex.Message}"
                };
            }
        }


        public async Task<ResponseDto?> GetRestaurantReviewAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.GET,
                Url = $"{SD.ReviewAPIBase}/api/review/restaurant/{id}"
            });
        }


        public async Task<ResponseDto?> DeleteRestaurantReviewAsync(int id)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.DELETE,
                Url = $"{SD.ReviewAPIBase}/api/restaurant/{id}"
            });
        }
    }
}
