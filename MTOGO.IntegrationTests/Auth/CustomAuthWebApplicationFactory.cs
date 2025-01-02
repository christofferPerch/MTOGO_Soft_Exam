using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MTOGO.Services.AuthAPI.Data;
using MTOGO.Services.AuthAPI.Models;

namespace MTOGO.IntegrationTests.Auth
{
    public class CustomAuthWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        public static string TestUserId { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryAuthTestDb");
                });

                using var scope = services.BuildServiceProvider().CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                dbContext.Database.EnsureCreated();

                SeedTestData(userManager).Wait();
            });
        }

        private async Task SeedTestData(UserManager<ApplicationUser> userManager)
        {
            var testUser = new ApplicationUser
            {
                UserName = "testuser",
                Email = "testuser@example.com",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "123-456-7890",
                Address = "123 Main St",
                City = "Metropolis",
                ZipCode = "12345",
                Country = "USA"
            };

            var existingUser = await userManager.FindByNameAsync(testUser.UserName);
            if (existingUser == null)
            {
                var result = await userManager.CreateAsync(testUser, "StrongP@ssword1");
                if (result.Succeeded)
                {
                    existingUser = await userManager.FindByNameAsync(testUser.UserName);
                }
            }

            if (existingUser != null)
            {
                TestUserId = existingUser.Id;
            }
        }
    }
}
