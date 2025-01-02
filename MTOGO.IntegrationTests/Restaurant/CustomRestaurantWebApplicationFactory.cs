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
        private readonly string _testConnectionString = "Server=localhost,1450;Database=RestaurantServiceTestDB;User Id=sa;Password=YourStrong@Password1;TrustServerCertificate=True;";

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
                // Replace IDataAccess with a Dapper SQL Server connection pointing to the test database
                services.AddScoped<IDataAccess>(_ => new DataAccess(_testConnectionString));

                // Seed the test database
                using var connection = new SqlConnection(_testConnectionString);
                connection.Open();
                SeedTestData(connection);
            });
        }

        private void SeedTestData(IDbConnection connection)
        {
            connection.Execute(@"
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Address' AND xtype='U')
        BEGIN
            CREATE TABLE Address (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                AddressLine1 NVARCHAR(255) NOT NULL,
                AddressLine2 NVARCHAR(255),
                City NVARCHAR(100) NOT NULL,
                ZipCode NVARCHAR(20) NOT NULL,
                Country NVARCHAR(100) NOT NULL
            );

            CREATE TABLE Restaurant (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                RestaurantName NVARCHAR(100) NOT NULL,
                LegalName NVARCHAR(255) NOT NULL,
                VATNumber NVARCHAR(50) NOT NULL,
                RestaurantDescription NVARCHAR(500),
                ContactEmail NVARCHAR(255) NOT NULL,
                ContactPhone NVARCHAR(50) NOT NULL,
                AddressId INT NOT NULL FOREIGN KEY REFERENCES Address(Id)
            );

            CREATE TABLE OperatingHours (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                RestaurantId INT NOT NULL FOREIGN KEY REFERENCES Restaurant(Id),
                DayOfWeek NVARCHAR(20) NOT NULL,
                OpeningTime TIME NOT NULL,
                ClosingTime TIME NOT NULL
            );

            INSERT INTO Address (AddressLine1, AddressLine2, City, ZipCode, Country)
            VALUES ('123 Test St', NULL, 'Test City', '12345', 'Test Country');

            INSERT INTO Restaurant (RestaurantName, LegalName, VATNumber, RestaurantDescription, ContactEmail, ContactPhone, AddressId)
            VALUES ('Test Restaurant', 'Legal Name', 'VAT123', 'A test restaurant', 'test@example.com', '123-456-7890', 1);

            INSERT INTO OperatingHours (RestaurantId, DayOfWeek, OpeningTime, ClosingTime)
            VALUES (1, 'Monday', '08:00:00', '22:00:00'),
                   (1, 'Tuesday', '08:00:00', '22:00:00'),
                   (1, 'Wednesday', '08:00:00', '22:00:00'),
                   (1, 'Thursday', '08:00:00', '22:00:00'),
                   (1, 'Friday', '08:00:00', '22:00:00'),
                   (1, 'Saturday', '08:00:00', '22:00:00'),
                   (1, 'Sunday', '08:00:00', '22:00:00');
        END
    ");
        }
    }
}
