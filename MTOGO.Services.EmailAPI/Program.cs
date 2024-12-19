using DotNetEnv;
using Microsoft.OpenApi.Models;
using MTOGO.MessageBus;
using MTOGO.Services.EmailAPI.Services;
using MTOGO.Services.EmailAPI.Services.IServices;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

builder.Services.AddHostedService<EmailQueueListener>(); 
builder.Services.AddScoped<IEmailService, EmailService>(); 
builder.Services.AddSingleton<IMessageBus, MessageBus>(); 

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Email API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Email API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
