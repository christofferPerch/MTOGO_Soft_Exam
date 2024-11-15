using MTOGO.Web.Models;

namespace MTOGO.Web.Services.IServices
{
    public interface IRestaurantService
    {
        Task<ResponseDto?> GetAllRestaurantsAsync();
    }
}
