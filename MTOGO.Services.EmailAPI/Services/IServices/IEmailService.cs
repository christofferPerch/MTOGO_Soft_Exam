using MTOGO.Services.EmailAPI.Models.Dto;

namespace MTOGO.Services.EmailAPI.Services.IServices {
    public interface IEmailService {
        Task<bool> SendOrderCreatedEmailAsync(OrderCreatedMessageDto order);
    }
}
