using Microsoft.OpenApi.Models;
using MTOGO.MessageBus;
using MTOGO.Services.ShoppingCartAPI.Extensions;
using MTOGO.Services.ShoppingCartAPI.Services;
using MTOGO.Services.ShoppingCartAPI.Services.IServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddSingleton<IMessageBus, MessageBus>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Shopping Cart API", Version = "v1" });
});

builder.AddAppAuthetication();

var app = builder.Build();

// Use middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shopping Cart API");
    });
}

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger/index.html", permanent: false);
    }
    else
    {
        await next();
    }
});


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
