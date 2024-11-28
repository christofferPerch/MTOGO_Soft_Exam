using MTOGO.Web.Models;
using MTOGO.Web.Models.Order;

namespace MTOGO.Web.Services.IServices
{
    public interface IOrderService
    {
        Task<ResponseDto?> ProcessPaymentAsync(PaymentRequestDto paymentRequest);

    }
}
