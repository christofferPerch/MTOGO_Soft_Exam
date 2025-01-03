using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

public class AuthAPITests {
    private readonly HttpClient _client;

    public AuthAPITests() {
        // Load configuration from appsettings.json
        var configuration = ConfigurationHelper.LoadConfiguration();
        var authApiBaseUrl = configuration["AuthApiBaseUrl"]; // Retrieve Auth API base URL

        _client = new HttpClient { BaseAddress = new Uri(authApiBaseUrl) };
    }

    [Fact]
    public async Task Register_NewUser_ShouldReturnSuccess() {
        var user = new {
            email = "newuser@example.com",
            firstName = "John",
            lastName = "Doe",
            address = "123 Main St",
            city = "Cityville",
            zipCode = "12345",
            country = "Countryland",
            phoneNumber = "1234567890",
            password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/Register", user);

        // Assert the response status and content
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Registration should succeed for valid data");

        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result.isSuccess.Should().BeTrue("Response should indicate success");
    }

    [Fact]
    public async Task Login_ValidCredentials_ShouldReturnToken() {
        var credentials = new {
            userName = "newuser@example.com", // Same user created in Register test
            password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/Login", credentials);

        // Assert the response status
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Login should succeed for valid credentials");

        // Deserialize the response to check for token
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseContent); // Log the raw response for debugging

        var jsonResponse = System.Text.Json.JsonDocument.Parse(responseContent);
        var isSuccess = jsonResponse.RootElement.GetProperty("isSuccess").GetBoolean();

        isSuccess.Should().BeTrue("The login response should indicate success");

        // Extract the token from the result field
        var token = jsonResponse.RootElement.GetProperty("result").GetProperty("token").GetString();
        token.Should().NotBeNullOrEmpty("Token should be returned on successful login");
    }

}
