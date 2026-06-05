using MassTransit;
using NotificationService.Consumers;
using MicroserviceShopDemo.Common.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register MassTransit
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedNotificationConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Dedicated queue for notifications
        cfg.ReceiveEndpoint("notification-order-created-queue", e =>
        {
            e.ConfigureConsumer<OrderCreatedNotificationConsumer>(context);
        });
    });
});

builder.WebHost.UseUrls("http://+:80");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();