﻿namespace MTOGO.Web.Models.ShoppingCart
{
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public int MenuItemId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
