using MTOGO.Services.EmailAPI.Services;
using MTOGO.Services.EmailAPI.Services.IServices;
using MTOGO.MessageBus;
using Microsoft.OpenApi.Models;
using MTOGO.Services.EmailAPI.Services.IServices;
using MTOGO.Services.EmailAPI.Services;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

// Register services
builder.Services.AddHostedService<EmailQueueListener>(); // Queue listener background service
builder.Services.AddScoped<IEmailService, EmailService>(); // Email Service
builder.Services.AddSingleton<IMessageBus, MessageBus>(); // RabbitMQ Message Bus

// Add configuration for Mailgun
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Email API", Version = "v1" });
});

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Email API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
