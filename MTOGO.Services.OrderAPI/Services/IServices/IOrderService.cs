using MTOGO.Services.OrderAPI.Models.Dto;

namespace MTOGO.Services.OrderAPI.Services.IServices
{
    public interface IOrderService
    {
        Task<PaymentResponseDto> ProcessPayment(PaymentRequestDto paymentRequest);
        Task<int> CreateOrder(AddOrderDto order);
        Task<OrderDto?> GetOrderById(int id);
        Task<int> UpdateOrderStatus(int orderId, int statusId);
        Task<List<OrderDto>> GetActiveOrders(string userId);
        Task<List<OrderDto>> GetOrderHistory(string userId);
    }
}
