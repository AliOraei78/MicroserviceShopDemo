using Serilog;
using Yarp.ReverseProxy;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/api-gateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register YARP Reverse Proxy
builder.Services.AddReverseProxy()
       .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.WebHost.UseUrls("http://+:80");

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MicroserviceShopDemo - API Gateway",
        Version = "v1",
        Description = "Single entry point for all online store services.",
        Contact = new OpenApiContact
        {
            Name = "Ali Jenabi"
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/products/v1/swagger.json", "Products API");
        options.SwaggerEndpoint("/swagger/customers/v1/swagger.json", "Customers API");
        options.SwaggerEndpoint("/swagger/orders/v1/swagger.json", "Orders API");
    });
}

//app.UseHttpsRedirection();

// Enable YARP Reverse Proxy
app.MapReverseProxy();

app.Run();