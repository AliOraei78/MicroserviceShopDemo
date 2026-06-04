using MassTransit;
using PaymentService.Models;
using MicroserviceShopDemo.Common.Events;

namespace PaymentService.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var orderEvent = context.Message;

        _logger.LogInformation(
            "Received new order event: OrderId = {OrderId}",
            orderEvent.OrderId);

        // Simulate payment processing
        await ProcessPaymentAsync(orderEvent);

        _logger.LogInformation(
            "Payment for Order {OrderId} was completed successfully.",
            orderEvent.OrderId);
    }

    private async Task ProcessPaymentAsync(OrderCreatedEvent orderEvent)
    {
        // In a real-world application, this is where integration
        // with a payment gateway (Stripe, PayPal, ZarinPal, NextPay, etc.)
        // would be implemented.

        await Task.Delay(2000); // Simulate payment processing delay

        // In the future, a PaymentCompletedEvent can be published here
    }
}