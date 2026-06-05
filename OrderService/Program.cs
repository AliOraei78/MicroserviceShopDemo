using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Interfaces;
using OrderService.Repositories;
using OrderService.Services;
using MicroserviceShopDemo.Common.Events;

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

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<OrderDomainService>();
builder.Services.AddHttpClient<OrderDomainService>(client =>
{
    var baseUrl = builder.Configuration["ProductServiceSettings:BaseUrl"] ?? "http://product-service/";
    client.BaseAddress = new Uri(baseUrl);
});
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

var app = builder.Build();

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    dbContext.Database.Migrate();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
