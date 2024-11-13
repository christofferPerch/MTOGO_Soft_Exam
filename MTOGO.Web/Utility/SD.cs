namespace MTOGO.Web.Utility
{
    public class SD
    {
        public static string AuthAPIBase { get; set; }
        public static string RestaurantAPIBase { get; set; }
        public static string ShoppingCartAPIBase { get; set; }
        public static string OrderAPIBase { get; set; }
        public static string ReviewAPIBase { get; set; }
        public const string RoleAdmin = "ADMIN";
        public const string RoleCustomer = "CUSTOMER";
        public const string RoleDeliveryAgent = "DELIVERYAGENT";
        public const string RoleRestaurantOwner = "RESTAURANTOWNER";
        public const string TokenCookie = "JWTToken";
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }
        public enum ContentType
        {
            Json,
            MultipartFormData,
        }
    }
}
