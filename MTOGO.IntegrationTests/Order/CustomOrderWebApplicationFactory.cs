using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Dapper;
using MTOGO.Services.DataAccess;
using MTOGO.MessageBus;
using MTOGO.IntegrationTests.Mocks;

namespace MTOGO.IntegrationTests.Order
{
    public class CustomOrderWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly string _testConnectionString;
        private static readonly object DbLock = new();

        public CustomOrderWebApplicationFactory()
        {
            var sqlServerHost = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "localhost";
            var sqlServerPort = Environment.GetEnvironmentVariable("SQLSERVER_PORT") ?? "1451"; // Updated to port 1451
            var sqlServerUser = Environment.GetEnvironmentVariable("SQLSERVER_USER") ?? "sa";
            var sqlServerPassword = Environment.GetEnvironmentVariable("SQLSERVER_PASSWORD") ?? "YourStrong@Password1";

            _testConnectionString = $"Server={sqlServerHost},{sqlServerPort};Database=OrderServiceTestDB;User Id={sqlServerUser};Password={sqlServerPassword};TrustServerCertificate=True;";
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var testSettings = new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = _testConnectionString
                };

                config.AddInMemoryCollection(testSettings);
            });

            builder.ConfigureServices(services =>
            {
                // Replace IMessageBus with a mock implementation
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IMessageBus));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddSingleton<IMessageBus, MockMessageBus>();

                // Register the database access layer
                services.AddScoped<IDataAccess>(_ => new DataAccess(_testConnectionString));

                // Reset and seed the database
                lock (DbLock)
                {
                    ResetAndSeedDatabase();
                }
            });
        }

        private void ResetAndSeedDatabase()
        {
            using var connection = new SqlConnection(_testConnectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                ResetDatabase(connection, transaction);
                SeedTestData(connection, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private void ResetDatabase(IDbConnection connection, IDbTransaction transaction)
        {
            connection.Execute(@"
                DELETE FROM OrderItem;
                DBCC CHECKIDENT ('OrderItem', RESEED, 0);

                DELETE FROM [Order];
                DBCC CHECKIDENT ('[Order]', RESEED, 0);
            ", transaction: transaction);
        }

        private void SeedTestData(IDbConnection connection, IDbTransaction transaction)
        {
            connection.Execute(@"
                -- Insert a sample active order
                INSERT INTO [Order] (UserId, TotalAmount, VATAmount, OrderPlacedTimestamp, OrderStatusId)
                VALUES ('TestUser', 100.00, 20.00, GETDATE(), 1);

                DECLARE @OrderId1 INT = SCOPE_IDENTITY();

                -- Insert related Order Items for the active order
                INSERT INTO OrderItem (OrderId, RestaurantId, MenuItemId, Price, Quantity)
                VALUES 
                (@OrderId1, 1, 1, 50.00, 1),
                (@OrderId1, 1, 2, 50.00, 1);

                -- Insert a sample completed order
                INSERT INTO [Order] (UserId, TotalAmount, VATAmount, OrderPlacedTimestamp, OrderStatusId)
                VALUES ('TestUser', 150.00, 30.00, GETDATE(), 3);

                DECLARE @OrderId2 INT = SCOPE_IDENTITY();

                -- Insert related Order Items for the completed order
                INSERT INTO OrderItem (OrderId, RestaurantId, MenuItemId, Price, Quantity)
                VALUES 
                (@OrderId2, 2, 3, 75.00, 2),
                (@OrderId2, 2, 4, 75.00, 1);
            ", transaction: transaction);
        }

    }
}
