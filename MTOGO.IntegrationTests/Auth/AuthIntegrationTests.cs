/* using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using MTOGO.Services.AuthAPI;
using MTOGO.Services.AuthAPI.Models.Dto;
using System.Net;

namespace MTOGO.IntegrationTests.Auth
{
    public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AuthIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                // Configure test-specific services here, if needed
            });
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Register_ValidUser_ReturnsOk()
        {
            var registrationRequest = new RegistrationRequestDto
            {
                Email = "testuser@example.com",
                Password = "Test@1234",
                FirstName = "Test",
                LastName = "User",
                Address = "123 Main St",
                City = "Test City",
                ZipCode = "12345",
                Country = "Test Country",
                PhoneNumber = "1234567890"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/Register", registrationRequest);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("isSuccess", content);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            var loginRequest = new LoginRequestDto
            {
                UserName = "testuser@example.com",
                Password = "Test@1234"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/Login", loginRequest);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

            Assert.NotNull(loginResponse);
            Assert.False(string.IsNullOrEmpty(loginResponse.Token));
        }

        [Fact]
        public async Task AssignRole_ValidRequest_ReturnsOk()
        {
            var assignRoleRequest = new AssignRoleDto
            {
                Email = "testuser@example.com",
                Role = "Admin"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/AssignRole", assignRoleRequest);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
*/