using Yarp.ReverseProxy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register YARP Reverse Proxy
builder.Services.AddReverseProxy()
       .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("https://localhost:7265/swagger/v1/swagger.json", "Products API");
        options.SwaggerEndpoint("https://localhost:7103/swagger/v1/swagger.json", "Customers API");
        options.SwaggerEndpoint("https://localhost:7260/swagger/v1/swagger.json", "Orders API");
    });}

app.UseHttpsRedirection();

// Enable YARP Reverse Proxy
app.MapReverseProxy();

app.Run();