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
            // Call ProductService to retrieve product pricing
            var response = await _httpClient.GetAsync(
                $"https://localhost:5029/api/products/{item.ProductId}");

            if (response.IsSuccessStatusCode)
            {
                var product = await response.Content.ReadFromJsonAsync<dynamic>();

                total += (decimal)product.price * item.Quantity;
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