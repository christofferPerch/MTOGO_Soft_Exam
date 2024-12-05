using Microsoft.OpenApi.Models;
using MTOGO.GatewaySolution.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.AddAppAuthetication();

string ocelotConfigFile = Path.Combine(Directory.GetCurrentDirectory(), "ocelot.json");
if (!File.Exists(ocelotConfigFile))
{
    Console.WriteLine($"ocelot.json not found at {ocelotConfigFile}");
    throw new FileNotFoundException($"Ocelot configuration file not found: {ocelotConfigFile}");
}
Console.WriteLine($"Loading Ocelot configuration from: {ocelotConfigFile}");
builder.Configuration.AddJsonFile(ocelotConfigFile, optional: false, reloadOnChange: true);




builder.Services.AddOcelot(builder.Configuration);

builder.Services.AddSwaggerForOcelot(builder.Configuration);
 var ocelot = builder.Build();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API Gateway",
        Version = "v1"
    });
});

builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

// Ocelot middleware
await app.UseOcelot();
// Redirect root to Swagger UI
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger/index.html", true);
    }
    else
    {
        await next();
    }
});



app.Run();
