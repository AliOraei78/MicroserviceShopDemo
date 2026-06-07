using InventoryService.Consumers;
using InventoryService.Data;
using InventoryService.Services;
using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using RabbitMQ.Client;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/inventory-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddGrpc();
builder.Services.AddControllers();

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });

    options.ListenAnyIP(50051, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Services.AddMassTransit(x =>
{
    // Register the consumer
    x.AddConsumer<ProductCreatedConsumer>();

    // Configure RabbitMQ transport
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        // Configure the endpoint/queue for this specific consumer
        cfg.ReceiveEndpoint("inventory-product-created-queue", e =>
        {
            e.ConfigureConsumer<ProductCreatedConsumer>(context);
        });
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<InventoryDbContext>("Database")
    .AddRabbitMQ(sp =>
    {
        // Create the connection using the RabbitMQ Client factory
        var factory = new ConnectionFactory { Uri = new Uri("amqp://guest:guest@rabbitmq:5672") };
        return factory.CreateConnectionAsync();
    }, name: "RabbitMQ");

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo // Using OpenApiInfo directly now
    {
        Title = "Inventory Service API",
        Version = "v1",
        Description = "Inventory service in a Microservices architecture.",
        Contact = new OpenApiContact // Using OpenApiContact directly now
        {
            Name = "Ali jenabi",
            Email = "a.jenabi78@example.com"
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    dbContext.Database.Migrate();
}

app.MapGrpcService<InventoryServiceImpl>();
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
});
if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => "gRPC Service is running. Use a gRPC client to call it.");
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "InventoryService v1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
    });
}

app.Run();