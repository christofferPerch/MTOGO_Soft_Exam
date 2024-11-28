using MTOGO.Web.Models;
using MTOGO.Web.Models.ShoppingCart;
using System.Threading.Tasks;

namespace MTOGO.Web.Services.IServices
{
    public interface IShoppingCartService
    {
        Task<ResponseDto?> AddItemToCartAsync(string userId, CartItem cartItem);
        Task<ResponseDto?> RemoveItemFromCartAsync(string userId, int menuItemId);
        Task<ResponseDto?> GetCartAsync(string userId);
        Task<ResponseDto?> ClearCartAsync(string userId);
        Task<ResponseDto?> SetCart(Cart cart);

    }
}
