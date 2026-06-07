using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using ProductService.Data;
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
    .WriteTo.File("logs/product-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMassTransit(x =>
{
    // If the ProductService needs to consume events, register consumers here:
    // x.AddConsumer<ProductStockUpdatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        // Use "rabbitmq" if running inside Docker Compose, or "localhost" if running locally
        var rabbitMqHost = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";

        cfg.Host(rabbitMqHost, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.WebHost.UseUrls("http://+:80");

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ProductDbContext>("Database");

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo // Using OpenApiInfo directly now
    {
        Title = "Product Service API",
        Version = "v1",
        Description = "Product service in a Microservices architecture.",
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
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CustomerService v1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
    });
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    dbContext.Database.Migrate();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();
