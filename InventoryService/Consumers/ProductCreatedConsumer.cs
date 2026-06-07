using InventoryService.Data;
using InventoryService.Models;
using MassTransit;
using MicroserviceShopDemo.Common.Events;

namespace InventoryService.Consumers;

public class ProductCreatedConsumer : IConsumer<ProductCreatedEvent>
{
    private readonly InventoryDbContext _context;
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(InventoryDbContext context, ILogger<ProductCreatedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        var message = context.Message;

        // Check if it already exists to prevent duplicate entries
        var exists = _context.InventoryItems.Any(i => i.ProductId == message.ProductId);
        if (!exists)
        {
            var newItem = new InventoryItem
            {
                ProductId = message.ProductId,
                Quantity = message.InitialStock,
                LastUpdated = DateTime.UtcNow
            };

            _context.InventoryItems.Add(newItem);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Inventory initialized for Product {ProductId} with {Stock} items.",
                message.ProductId,
                message.InitialStock);
        }
    }
}