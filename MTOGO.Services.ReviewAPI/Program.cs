using MTOGO.MessageBus;
using MTOGO.Services.ReviewAPI.Services;
using MTOGO.Services.ReviewAPI.Services.IServices;
using MTOGO.Services.DataAccess;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddScoped<IDataAccess, DataAccess>(sp =>
    new DataAccess(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddSingleton<IMessageBus, MessageBus>();

// Configure Redis caching
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
});

// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Review API", Version = "v1" });
});

var app = builder.Build();

// Use middleware
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Review API");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();