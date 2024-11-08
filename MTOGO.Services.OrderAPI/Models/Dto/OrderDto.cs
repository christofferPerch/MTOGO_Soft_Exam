﻿namespace MTOGO.Services.OrderAPI.Models.Dto
{
    public class OrderDto
    {
        public string UserId { get; set; }
        public int? DeliveryAgentId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal VATAmount { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }
}
