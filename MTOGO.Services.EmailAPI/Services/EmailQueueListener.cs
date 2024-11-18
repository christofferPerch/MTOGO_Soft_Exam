using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MTOGO.MessageBus;
using MTOGO.Services.EmailAPI.Models.Dto;
using MTOGO.Services.EmailAPI.Services.IServices;
using System.Threading;
using System.Threading.Tasks;

public class EmailQueueListener : BackgroundService {
    private readonly ILogger<EmailQueueListener> _logger;
    private readonly IMessageBus _messageBus;
    private readonly IServiceProvider _serviceProvider;
    private const string OrderCreatedQueue = "OrderCreatedQueue";

    public EmailQueueListener(ILogger<EmailQueueListener> logger, IMessageBus messageBus, IServiceProvider serviceProvider) {
        _logger = logger;
        _messageBus = messageBus;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _logger.LogInformation("Starting EmailQueueListener...");

        _messageBus.SubscribeMessage<OrderCreatedMessageDto>(OrderCreatedQueue, async (order) => {
            if (order != null) {
                _logger.LogInformation($"Received order created message for Order ID: {order.OrderId}");
                await HandleOrderCreatedMessageAsync(order);
            } else {
                _logger.LogWarning("Received a null order created message.");
            }
        });

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleOrderCreatedMessageAsync(OrderCreatedMessageDto order) {
        using var scope = _serviceProvider.CreateScope(); // Create a new scope
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>(); // Resolve IEmailService

        try {
            var emailSent = await emailService.SendOrderCreatedEmailAsync(order);
            if (emailSent) {
                _logger.LogInformation("Email sent successfully.");
            } else {
                _logger.LogWarning("Failed to send the email.");
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "Error handling OrderCreatedMessage.");
        }
    }
}