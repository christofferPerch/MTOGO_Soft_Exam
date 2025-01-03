using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using MTOGO.Services.DataAccess;

namespace MTOGO.IntegrationTests.Restaurant
{
    public class CustomRestaurantWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly string _testConnectionString;
        private static readonly object DbLock = new();

        public CustomRestaurantWebApplicationFactory()
        {
            var sqlServerHost = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "localhost";
            var sqlServerPort = Environment.GetEnvironmentVariable("SQLSERVER_PORT") ?? "1450";
            var sqlServerUser = Environment.GetEnvironmentVariable("SQLSERVER_USER") ?? "sa";
            var sqlServerPassword = Environment.GetEnvironmentVariable("SQLSERVER_PASSWORD") ?? "YourStrong@Password1";

            _testConnectionString = $"Server={sqlServerHost},{sqlServerPort};Database=RestaurantServiceTestDB;User Id={sqlServerUser};Password={sqlServerPassword};TrustServerCertificate=True;";
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
            DELETE FROM MenuItem;
            DBCC CHECKIDENT ('MenuItem', RESEED, 0);

            DELETE FROM FoodCategory;
            DBCC CHECKIDENT ('FoodCategory', RESEED, 0);

            DELETE FROM OperatingHours;
            DBCC CHECKIDENT ('OperatingHours', RESEED, 0);

            DELETE FROM Restaurant;
            DBCC CHECKIDENT ('Restaurant', RESEED, 0);

            DELETE FROM Address;
            DBCC CHECKIDENT ('Address', RESEED, 0);
        ", transaction: transaction);
        }

        private void SeedTestData(IDbConnection connection, IDbTransaction transaction)
        {
            connection.Execute(@"
            INSERT INTO Address (AddressLine1, AddressLine2, City, ZipCode, Country)
            VALUES ('123 Test St', NULL, 'Test City', '12345', 'Test Country');

            DECLARE @AddressId INT = SCOPE_IDENTITY();

            INSERT INTO Restaurant (RestaurantName, LegalName, VATNumber, RestaurantDescription, ContactEmail, ContactPhone, AddressId)
            VALUES ('Test Restaurant', 'Legal Name', 'VAT123', 'A test restaurant', 'test@example.com', '123-456-7890', @AddressId);

            DECLARE @RestaurantId INT = SCOPE_IDENTITY();

            INSERT INTO OperatingHours (RestaurantId, Day, OpeningHours, ClosingHours)
            VALUES 
            (@RestaurantId, 1, '09:00:00', '18:00:00'),
            (@RestaurantId, 2, '09:00:00', '18:00:00'),
            (@RestaurantId, 3, '09:00:00', '18:00:00');

            INSERT INTO FoodCategory (RestaurantId, Category)
            VALUES 
            (@RestaurantId, 1),
            (@RestaurantId, 2),
            (@RestaurantId, 3);

            INSERT INTO MenuItem (RestaurantId, Name, Description, Price, Image)
            VALUES 
            (@RestaurantId, 'Pizza', 'Delicious cheese pizza', 9.99, NULL),
            (@RestaurantId, 'Burger', 'Classic beef burger', 7.99, NULL),
            (@RestaurantId, 'Pasta', 'Creamy Alfredo pasta', 12.99, NULL);
        ", transaction: transaction);
        }
    }

}
