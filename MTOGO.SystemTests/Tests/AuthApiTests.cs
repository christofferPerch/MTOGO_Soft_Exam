using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

public class AuthAPITests
{
    private readonly HttpClient _client;

    public AuthAPITests()
    {
        var configuration = ConfigurationHelper.LoadConfiguration();
        var authApiBaseUrl = configuration["AuthApiBaseUrl"];

        _client = new HttpClient { BaseAddress = new Uri(authApiBaseUrl) };
    }

    [Fact]
    public async Task Register_NewUser_ShouldReturnSuccess()
    {
        var uniqueEmail = $"newuser-{Guid.NewGuid()}@example.com";

        var user = new
        {
            email = uniqueEmail,
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

        response.StatusCode.Should().Be(HttpStatusCode.OK, "Registration should succeed for valid data");

        var responseContent = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonDocument.Parse(responseContent);

        jsonResponse.RootElement.GetProperty("isSuccess").GetBoolean().Should().BeTrue("Response should indicate success");
    }

    [Fact]
    public async Task Login_ValidCredentials_ShouldReturnToken()
    {
        var credentials = new
        {
            userName = "newuser@example.com",
            password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/Login", credentials);

        response.StatusCode.Should().Be(HttpStatusCode.OK, "Login should succeed for valid credentials");

        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseContent);

        var jsonResponse = System.Text.Json.JsonDocument.Parse(responseContent);
        var isSuccess = jsonResponse.RootElement.GetProperty("isSuccess").GetBoolean();

        isSuccess.Should().BeTrue("The login response should indicate success");

        var token = jsonResponse.RootElement.GetProperty("result").GetProperty("token").GetString();
        token.Should().NotBeNullOrEmpty("Token should be returned on successful login");
    }
}
