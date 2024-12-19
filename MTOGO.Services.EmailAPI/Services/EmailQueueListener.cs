using MTOGO.MessageBus;
using MTOGO.Services.EmailAPI.Models.Dto;
using MTOGO.Services.EmailAPI.Services.IServices;

public class EmailQueueListener : BackgroundService
{
    private readonly IMessageBus _messageBus;
    private readonly IServiceProvider _serviceProvider;
    private const string OrderCreatedQueue = "OrderCreatedQueue";

    public EmailQueueListener(IMessageBus messageBus, IServiceProvider serviceProvider)
    {
        _messageBus = messageBus;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _messageBus.SubscribeMessage<OrderCreatedMessageDto>(OrderCreatedQueue, async (order) =>
        {
            if (order != null)
            {
                await HandleOrderCreatedMessageAsync(order);
            }
            else
            {
                throw new Exception();
            }
        });

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleOrderCreatedMessageAsync(OrderCreatedMessageDto order)
    {
        using var scope = _serviceProvider.CreateScope();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        try
        {
            var emailSent = await emailService.SendOrderCreatedEmailAsync(order);
        }
        catch (Exception)
        {
            throw;
        }
    }
}