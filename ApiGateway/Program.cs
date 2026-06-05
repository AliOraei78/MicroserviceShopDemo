using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register YARP Reverse Proxy
builder.Services.AddReverseProxy()
       .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.WebHost.UseUrls("http://+:80");

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