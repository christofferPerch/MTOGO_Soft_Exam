namespace MTOGO.Web.Models.ShoppingCart
{
    public class Cart
    {
        public string UserId { get; set; }
        public List<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
