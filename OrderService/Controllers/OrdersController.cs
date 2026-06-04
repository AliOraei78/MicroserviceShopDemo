using Microsoft.AspNetCore.Mvc;
using OrderService.DTOs;
using OrderService.Interfaces;
using OrderService.Models;
using OrderService.Services;

namespace OrderService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IOrderRepository _repository;
    private readonly OrderDomainService _domainService;

    public OrdersController(IOrderRepository repository, OrderDomainService domainService)
    {
        _repository = repository;
        _domainService = domainService;
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(CreateOrderDto dto)
    {
        _domainService.ValidateOrder(dto);

        var order = new Order
        {
            CustomerId = dto.CustomerId,
            TotalAmount = await _domainService.CalculateTotalAsync(dto.Items),
            OrderDate = DateTime.UtcNow,
            Status = "Pending"
        };

        foreach (var item in dto.Items)
        {
            order.OrderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = 0
            });
        }

        await _repository.AddAsync(order);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        return Ok(await _repository.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null) return NotFound();
        return order;
    }
}