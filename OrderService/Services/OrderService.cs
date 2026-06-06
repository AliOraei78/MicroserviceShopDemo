using OrderService.DTOs;
using OrderService.Models;

namespace OrderService.Services;

public class OrderDomainService
{
    private readonly HttpClient _httpClient;

    public OrderDomainService(HttpClient httpClient)
    {
        _httpClient = httpClient;
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