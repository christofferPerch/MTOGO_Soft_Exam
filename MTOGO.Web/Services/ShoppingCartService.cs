using MTOGO.Web.Models;
using MTOGO.Web.Models.ShoppingCart;
using MTOGO.Web.Services.IServices;
using MTOGO.Web.Utility;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace MTOGO.Web.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IBaseService _baseService;
        public ShoppingCartService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> AddItemToCartAsync(string userId, CartItem cartItem)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Data = new { UserId = userId, Item = cartItem },
                Url = SD.ShoppingCartAPIBase + "/api/shoppingcart/add"
            });
        }

        public async Task<ResponseDto?> RemoveItemFromCartAsync(string userId, int menuItemId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.DELETE,
                Url = $"{SD.ShoppingCartAPIBase}/api/shoppingcart?userId={userId}&menuItemId={menuItemId}" 
            });
        }


        public async Task<ResponseDto?> GetCartAsync(string userId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{SD.ShoppingCartAPIBase}/api/shoppingcart/{userId}" 
            });
        }


        public async Task<ResponseDto?> ClearCartAsync(string userId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.DELETE,
                Url = $"{SD.ShoppingCartAPIBase}/api/shoppingcart/clear?userId={userId}"
            });
        }

        public async Task<ResponseDto?> SetCart(Cart cart)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Url = $"{SD.ShoppingCartAPIBase}/api/shoppingcart/set",
                Data = cart
            });
        }
    }
}
