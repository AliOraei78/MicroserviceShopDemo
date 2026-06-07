namespace MicroserviceShopDemo.Common.Events;

public class ProductCreatedEvent
{
    public int ProductId { get; set; }
    public int InitialStock { get; set; }
}