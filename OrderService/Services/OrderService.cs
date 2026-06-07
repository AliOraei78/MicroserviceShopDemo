using Grpc.Net.Client;
using InventoryService.Protos;
using OrderService.DTOs;
using OrderService.Models;
using System.Threading.Tasks;

namespace OrderService.Services;

public class OrderDomainService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderDomainService> _logger;

    public OrderDomainService(
        HttpClient httpClient,
        ILogger<OrderDomainService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    // New method: Check and reserve stock via gRPC
    public async Task<bool> CheckAndReserveStockAsync(List<OrderItemDto> items)
    {
        using var channel = GrpcChannel.ForAddress("http://inventory-service:50051"); // InventoryService port
        var client = new InventoryService.Protos.InventoryService.InventoryServiceClient(channel);

        foreach (var item in items)
        {
            try
            {
                // Step 1: Check stock availability
                var stockRequest = new StockRequest
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };

                var stockResponse = await client.CheckStockAsync(stockRequest);

                if (!stockResponse.IsAvailable)
                {
                    _logger.LogWarning(
                        "Insufficient stock for Product {ProductId}. Current stock: {Stock}",
                        item.ProductId,
                        stockResponse.CurrentStock);

                    return false;
                }

                // Step 2: Reserve stock
                var reserveRequest = new ReserveRequest
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };

                var reserveResponse = await client.ReserveStockAsync(reserveRequest);

                if (!reserveResponse.Success)
                {
                    _logger.LogWarning(
                        "Stock reservation failed: {Message}",
                        reserveResponse.Message);

                    return false;
                }

                _logger.LogInformation(
                    "Stock successfully reserved for Product {ProductId}.",
                    item.ProductId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "gRPC communication error with InventoryService for Product {ProductId}",
                    item.ProductId);

                return false;
            }
        }

        return true;
    }


    public async Task<decimal> CalculateTotalAsync(List<OrderItemDto> items)
    {
        decimal total = 0;

        foreach (var item in items)
        {
            try
            {
                // Call ProductService to retrieve product pricing
                var response = await _httpClient.GetAsync($"api/products/{item.ProductId}");

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize directly into the ProductDto class
                    var product = await response.Content.ReadFromJsonAsync<ProductDto>();

                    if (product != null)
                    {
                        total += product.Price * item.Quantity;
                    }
                }
            }
            catch(Exception ex)
            {
                // Log the exception and continue with the next item
                Console.WriteLine($"Error fetching product {item.ProductId}: {ex.Message}");
            }
        }

        return total;
    }

    public void ValidateOrder(CreateOrderDto orderDto)
    {
        if (orderDto.CustomerId <= 0)
            throw new ArgumentException("Invalid CustomerId.");

        if (!orderDto.Items.Any())
            throw new ArgumentException(
                "An order must contain at least one product.");
    }
}