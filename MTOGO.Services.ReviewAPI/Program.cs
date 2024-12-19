using Microsoft.OpenApi.Models;
using MTOGO.MessageBus;
using MTOGO.Services.DataAccess;
using MTOGO.Services.ReviewAPI.Extensions;
using MTOGO.Services.ReviewAPI.Services;
using MTOGO.Services.ReviewAPI.Services.IServices;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddScoped<IDataAccess, DataAccess>(sp =>
    new DataAccess(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddSingleton<IMessageBus, MessageBus>();

// Configure Redis caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
});

// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Review API", Version = "v1" });
});

builder.AddAppAuthetication();

var app = builder.Build();

// Use middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Review API");
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