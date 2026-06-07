using Grpc.Core;
using InventoryService.Data;
using InventoryService.Models;
using InventoryService.Protos;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Services;

public class InventoryServiceImpl : Protos.InventoryService.InventoryServiceBase
{
    private readonly InventoryDbContext _context;
    private readonly ILogger<InventoryServiceImpl> _logger;

    public InventoryServiceImpl(InventoryDbContext context, ILogger<InventoryServiceImpl> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override async Task<StockResponse> CheckStock(StockRequest request, ServerCallContext context)
    {
        var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == request.ProductId);

        if (item == null || item.Quantity < request.Quantity)
            return new StockResponse { IsAvailable = false, CurrentStock = item?.Quantity ?? 0 };

        return new StockResponse { IsAvailable = true, CurrentStock = item.Quantity };
    }

    public override async Task<ReserveResponse> ReserveStock(ReserveRequest request, ServerCallContext context)
    {
        var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.ProductId == request.ProductId);

        if (item == null || item.Quantity < request.Quantity)
            return new ReserveResponse { Success = false, Message = "Insufficient stock available." };

        item.Quantity -= request.Quantity;
        item.LastUpdated = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Reserved {Quantity} units of Product {ProductId}.", request.ProductId, request.Quantity);
        return new ReserveResponse { Success = true, Message = "Stock reserved successfully." };
    }
}