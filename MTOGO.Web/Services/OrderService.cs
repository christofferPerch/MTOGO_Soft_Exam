using MTOGO.Web.Models;
using MTOGO.Web.Models.Order;
using MTOGO.Web.Services.IServices;
using MTOGO.Web.Utility;

namespace MTOGO.Web.Services
{
    public class OrderService : IOrderService
    {
        private readonly IBaseService _baseService;

        public OrderService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> ProcessPaymentAsync(PaymentRequestDto paymentRequest)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.POST,
                Url = $"{SD.OrderAPIBase}/api/order/processPayment",
                Data = paymentRequest
            });
        }

        public async Task<ResponseDto?> GetActiveOrders(string userId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.GET,
                Url = $"{SD.OrderAPIBase}/api/order/active-orders/{userId}"
            });
        }

        public async Task<ResponseDto?> GetOrderHistory(string userId)
        {
            return await _baseService.SendAsync(new RequestDto
            {
                ApiType = SD.ApiType.GET,
                Url = $"{SD.OrderAPIBase}/api/order/order-history/{userId}"
            });
        }

    }
}
