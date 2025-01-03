using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using MTOGO.MessageBus;
using MTOGO.IntegrationTests.Mocks;

namespace MTOGO.IntegrationTests.ShoppingCart
{
    public class CustomShoppingCartWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly string _testRedisConnectionString;

        public CustomShoppingCartWebApplicationFactory()
        {
            var redisHost = Environment.GetEnvironmentVariable("TESTREDIS_HOST") ?? "localhost";
            var redisPort = Environment.GetEnvironmentVariable("TESTREDIS_PORT") ?? "6380";
            _testRedisConnectionString = $"{redisHost}:{redisPort}";
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var testSettings = new Dictionary<string, string>
                {
                    ["Redis:ConnectionString"] = _testRedisConnectionString
                };

                config.AddInMemoryCollection(testSettings);
            });

            builder.ConfigureServices(services =>
            {
                // Replace IConnectionMultiplexer for Redis
                var redisDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionMultiplexer));
                if (redisDescriptor != null)
                {
                    services.Remove(redisDescriptor);
                }
                try
                {
                    var redis = ConnectionMultiplexer.Connect(_testRedisConnectionString);
                    services.AddSingleton<IConnectionMultiplexer>(redis);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Redis Connection Error: {ex.Message}");
                    throw;
                }

                // Replace IMessageBus with MockMessageBus
                var messageBusDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMessageBus));
                if (messageBusDescriptor != null)
                {
                    services.Remove(messageBusDescriptor);
                }
                services.AddSingleton<IMessageBus, MockMessageBus>();
            });
        }
    }
}
