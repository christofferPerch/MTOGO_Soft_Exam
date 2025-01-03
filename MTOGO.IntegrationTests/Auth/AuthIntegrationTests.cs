using System.Net.Http.Json;
using FluentAssertions;
using MTOGO.Services.AuthAPI.Models.Dto;

namespace MTOGO.IntegrationTests.Auth
{
    [Trait("Category", "Auth")]
    public class AuthIntegrationTests : IClassFixture<CustomAuthWebApplicationFactory<MTOGO.Services.AuthAPI.Program>>
    {
        private readonly HttpClient _client;

        public AuthIntegrationTests(CustomAuthWebApplicationFactory<MTOGO.Services.AuthAPI.Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_ValidInput_ShouldReturnOk()
        {
            var registrationRequest = new RegistrationRequestDto
            {
                Email = "testuser@example.com",
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Main St",
                City = "Metropolis",
                ZipCode = "12345",
                Country = "USA",
                PhoneNumber = "123-456-7890",
                Password = "StrongP@ssword1"
            };

            var response = await _client.PostAsJsonAsync("api/auth/Register", registrationRequest);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Register_MissingRequiredFields_ShouldReturnBadRequest()
        {
            var invalidRegistrationRequest = new RegistrationRequestDto
            {
                Email = "",
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Main St",
                City = "Metropolis",
                ZipCode = "12345",
                Country = "USA",
                PhoneNumber = "123-456-7890",
                Password = "StrongP@ssword1"
            };

            var response = await _client.PostAsJsonAsync("api/auth/Register", invalidRegistrationRequest);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_ValidCredentials_ShouldReturnOk()
        {
            var loginRequest = new LoginRequestDto
            {
                UserName = "testuser",
                Password = "StrongP@ssword1"
            };

            var response = await _client.PostAsJsonAsync("api/auth/Login", loginRequest);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var loginResponse = await response.Content.ReadFromJsonAsync<ResponseDto>();
            loginResponse.Should().NotBeNull();
            loginResponse!.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Login_InvalidCredentials_ShouldReturnBadRequest()
        {

            var loginRequest = new LoginRequestDto
            {
                UserName = "nonexistentuser",
                Password = "WrongPassword123!"
            };

            var response = await _client.PostAsJsonAsync("api/auth/Login", loginRequest);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            var responseContent = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseContent.Should().NotBeNull();
            responseContent!.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task AssignRole_ValidInput_ShouldReturnOk()
        {
            var assignRoleRequest = new AssignRoleDto
            {
                Email = "testuser@example.com",
                Role = "Admin"
            };

            var response = await _client.PostAsJsonAsync("api/auth/AssignRole", assignRoleRequest);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task AssignRole_NonExistentUser_ShouldReturnBadRequest()
        {
            var assignRoleRequest = new AssignRoleDto
            {
                Email = "nonexistent@example.com",
                Role = "Admin"
            };

            var response = await _client.PostAsJsonAsync("api/auth/AssignRole", assignRoleRequest);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            var responseContent = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseContent.Should().NotBeNull();
            responseContent!.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateProfile_ValidInput_ShouldReturnOk()
        {
            var updateProfileDto = new UpdateProfileDto
            {
                Address = "789 Updated St",
                City = "UpdatedCity",
                ZipCode = "UpdatedZip",
                Country = "UpdatedCountry",
                PhoneNumber = "UpdatedPhone"
            };

            var userId = CustomAuthWebApplicationFactory<MTOGO.Services.AuthAPI.Program>.TestUserId;

            var response = await _client.PostAsJsonAsync($"api/auth/UpdateProfile?userId={userId}", updateProfileDto);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task UpdateProfile_MissingUserId_ShouldReturnBadRequest()
        {
            var updateProfileDto = new UpdateProfileDto
            {
                Address = "789 Updated St",
                City = "UpdatedCity",
                ZipCode = "UpdatedZip",
                Country = "UpdatedCountry",
                PhoneNumber = "UpdatedPhone"
            };

            var response = await _client.PostAsJsonAsync("api/auth/UpdateProfile", updateProfileDto);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DeleteProfile_ValidUserId_ShouldReturnOk()
        {
            var userId = CustomAuthWebApplicationFactory<MTOGO.Services.AuthAPI.Program>.TestUserId;

            var response = await _client.DeleteAsync($"api/auth/DeleteProfile?userId={userId}");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task DeleteProfile_InvalidUserId_ShouldReturnBadRequest()
        {
            var response = await _client.DeleteAsync("api/auth/DeleteProfile?userId=invalid-user-id");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

    }
}
