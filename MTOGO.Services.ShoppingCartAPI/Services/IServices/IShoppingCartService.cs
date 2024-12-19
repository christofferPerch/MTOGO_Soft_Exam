using MTOGO.Services.ShoppingCartAPI.Models;

namespace MTOGO.Services.ShoppingCartAPI.Services.IServices
{
    public interface IShoppingCartService
    {
        Task<Cart?> CreateCart(Cart cart);
        Task<Cart?> GetCart(string userId);
        Task<bool> RemoveCart(string userId);
        Task<bool> RemoveMenuItem(string userId, int menuItemId);
        Task<Cart?> SetCart(Cart cart);

    }
}
