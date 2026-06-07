using MassTransit;
using MicroserviceShopDemo.Common.Events;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OrderService.Data;
using OrderService.Interfaces;
using OrderService.Repositories;
using OrderService.Services;
using Polly;
using Polly.Extensions.Http;
using RabbitMQ.Client;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/order-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<OrderDomainService>();
// Combine base URL configuration and Polly resilience into a single chain
builder.Services.AddHttpClient<OrderDomainService>(client =>
{
    var baseUrl = builder.Configuration["ProductServiceSettings:BaseUrl"] ?? "http://product-service/";
    client.BaseAddress = new Uri(baseUrl);
})
.AddPolicyHandler(GetRetryPolicy())
.AddPolicyHandler(GetCircuitBreakerPolicy());

builder.Services.AddMassTransit(x =>
{
    // Configure RabbitMQ transport
    x.UsingRabbitMq((context, cfg) =>
    {
        // Set up RabbitMQ host connection
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.WebHost.UseUrls("http://+:80");

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrderDbContext>("Database")
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
        Title = "Order Service API",
        Version = "v1",
        Description = "Order service in a Microservices architecture.",
        Contact = new OpenApiContact // Using OpenApiContact directly now
        {
            Name = "Ali jenabi",
            Email = "a.jenabi78@example.com"
        }
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderService v1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
    });
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    dbContext.Database.Migrate();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Health Check Endpoints
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

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}