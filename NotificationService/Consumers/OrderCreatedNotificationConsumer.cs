using MassTransit;
using Microsoft.Extensions.Logging;
using MicroserviceShopDemo.Common.Events;

namespace NotificationService.Consumers;

public class OrderCreatedNotificationConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedNotificationConsumer> _logger;

    public OrderCreatedNotificationConsumer(
        ILogger<OrderCreatedNotificationConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var orderEvent = context.Message;

        _logger.LogInformation("=== Notification process started ===");
        _logger.LogInformation(
            "A new order with ID {OrderId} has been created for customer {CustomerId}.",
            orderEvent.OrderId,
            orderEvent.CustomerId);

        // Simulate email notification
        await SendEmailNotificationAsync(orderEvent);

        // Simulate SMS notification
        await SendSmsNotificationAsync(orderEvent);

        _logger.LogInformation("Notifications were sent successfully.");
    }

    private async Task SendEmailNotificationAsync(OrderCreatedEvent orderEvent)
    {
        // Simulate email delivery delay
        await Task.Delay(800);

        _logger.LogInformation(
            "📧 Email sent to customer: Order {OrderId}, Amount {TotalAmount}",
            orderEvent.OrderId,
            orderEvent.TotalAmount);

        // In a real-world project, SendGrid or MailKit would be used here
    }

    private async Task SendSmsNotificationAsync(OrderCreatedEvent orderEvent)
    {
        await Task.Delay(500);

        _logger.LogInformation(
            "📱 SMS sent to customer: Your order has been successfully registered.");
    }
}