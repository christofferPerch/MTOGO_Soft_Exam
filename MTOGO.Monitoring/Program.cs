using Prometheus;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


app.UseRouting();
app.UseHttpMetrics(); 

app.UseEndpoints(endpoints =>
{
    endpoints.MapMetrics(); 
});

app.Run();
