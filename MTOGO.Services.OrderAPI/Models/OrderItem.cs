﻿namespace MTOGO.Services.OrderAPI.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int RestaurantId { get; set; }
        public int MenuItemId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
