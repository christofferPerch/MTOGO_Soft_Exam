﻿namespace MTOGO.Services.OrderAPI.Models.Dto
{
    public class PaymentRequestDto
    {
        public string UserId { get; set; }
        public string CustomerEmail { get; set; }
        public Guid CorrelationId { get; set; }
        public List<CartItem> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CVV { get; set; }
    }
}
