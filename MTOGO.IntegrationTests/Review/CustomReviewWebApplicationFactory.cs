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

namespace MTOGO.IntegrationTests.Review
{
    public class CustomReviewWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly string _testConnectionString;
        private static readonly object DbLock = new();

        public CustomReviewWebApplicationFactory()
        {
            var sqlServerHost = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "localhost";
            var sqlServerPort = Environment.GetEnvironmentVariable("SQLSERVER_PORT") ?? "1452"; 
            var sqlServerUser = Environment.GetEnvironmentVariable("SQLSERVER_USER") ?? "sa";
            var sqlServerPassword = Environment.GetEnvironmentVariable("SQLSERVER_PASSWORD") ?? "YourStrong@Password1";

            _testConnectionString = $"Server={sqlServerHost},{sqlServerPort};Database=ReviewServiceTestDB;User Id={sqlServerUser};Password={sqlServerPassword};TrustServerCertificate=True;";
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

                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IMessageBus));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddSingleton<IMessageBus, MockMessageBus>();

                services.AddScoped<IDataAccess>(_ => new DataAccess(_testConnectionString));

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
        -- Delete all records from RestaurantReview
        DELETE FROM [RestaurantReview];
        -- Reset the identity column to start at 1
        DBCC CHECKIDENT ('[RestaurantReview]', RESEED, 0);
    ", transaction: transaction);
        }

        private void SeedTestData(IDbConnection connection, IDbTransaction transaction)
        {
            connection.Execute(@"
        -- Insert sample reviews into the RestaurantReview table
        INSERT INTO [RestaurantReview] (CustomerId, FoodRating, Comments, ReviewTimestamp, RestaurantId)
        VALUES 
        ('TestCustomer1', 5, 'Excellent food and service!', GETDATE(), 1),
        ('TestCustomer2', 4, 'Great food but slightly slow service.', GETDATE(), 2),
        ('TestCustomer3', 3, 'Average experience overall.', GETDATE(), 3),
        ('TestCustomer4', 2, 'Not impressed with the quality of the food.', GETDATE(), 1),
        ('TestCustomer5', 1, 'Terrible experience. Will not return.', GETDATE(), 4);
    ", transaction: transaction);
        }
    }
}
