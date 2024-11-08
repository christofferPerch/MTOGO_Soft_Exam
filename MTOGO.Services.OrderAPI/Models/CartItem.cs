﻿namespace MTOGO.Services.OrderAPI.Models
{
    public class CartItem
    {
        public int RestaurantId { get; set; }
        public int MenuItemId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
