using MassTransit;
using MicroserviceShopDemo.Common.Events;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using NotificationService.Consumers;
using RabbitMQ.Client;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/notification-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

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

builder.Services.AddHealthChecks()
    .AddRabbitMQ(sp =>
    {
        // Create the connection using the RabbitMQ Client factory
        var factory = new ConnectionFactory { Uri = new Uri("amqp://guest:guest@rabbitmq:5672") };
        return factory.CreateConnectionAsync();
    }, name: "RabbitMQ")
    .AddCheck("self", () => HealthCheckResult.Healthy("Notification Service is running"), tags: new[] { "ready" });

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo // Using OpenApiInfo directly now
    {
        Title = "Notification Service API",
        Version = "v1",
        Description = "Notification service in a Microservices architecture.",
        Contact = new OpenApiContact // Using OpenApiContact directly now
        {
            Name = "Ali jenabi",
            Email = "a.jenabi78@example.com"
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NotificationService v1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            details = report.Entries.Select(e => new
            {
                service = e.Key,
                status = e.Value.Status.ToString(),
                error = e.Value.Exception?.Message
            })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
}); app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapControllers();

app.Run();