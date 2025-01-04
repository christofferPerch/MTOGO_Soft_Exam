using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using MTOGO.Services.OrderAPI.Services.IServices;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

public class CustomerOrderAcceptanceTests {
    private readonly IDistributedCache _redisCache;
    private readonly HttpClient _client;

    public CustomerOrderAcceptanceTests() {

        _client = new HttpClient { BaseAddress = new Uri("http://localhost:7777") };

        var serviceProvider = new ServiceCollection()
            .AddStackExchangeRedisCache(options => {
                options.Configuration = "localhost:6379"; // Redis connection string
                options.InstanceName = "AcceptanceTests_";
            })
            .BuildServiceProvider();

        _redisCache = serviceProvider.GetRequiredService<IDistributedCache>();
    }

    [Fact]
    public async Task CustomerCanPlaceOrder_ShouldReturnOrderId() {
        // Step 1: Add to cart in Redis
        var cartKey = "user_test";
        var cart = new {
            UserId = cartKey,
            Items = new[]
            {
                new { RestaurantId = 1, MenuItemId = 1, Quantity = 2, Price = 50.00 }
            }
        };

        await _redisCache.SetStringAsync(cartKey, JsonConvert.SerializeObject(cart));

        // Step 2: Call the API and validate order creation (existing test logic)
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:7777") };
        var orderPayload = new {
            userId = cartKey,
            correlationId = Guid.NewGuid(),
            totalAmount = 100,
            items = cart.Items,
            cardNumber = "4111111111111111",
            expiryDate = "12/25",
            cvv = "123",
            customerEmail = "test@example.com"
        };

        var orderResponse = await httpClient.PostAsync(
            "/order/create",
            new StringContent(JsonConvert.SerializeObject(orderPayload), Encoding.UTF8, "application/json")
        );

        Assert.True(orderResponse.IsSuccessStatusCode, "Order creation failed");
    }
    [Fact]
    public async Task UpdateOrderStatus_ShouldSucceed() {
        // Step 1: Create an order
        var orderPayload = new {
            userId = "user_test",
            correlationId = Guid.NewGuid(),
            totalAmount = 100.00m,
            items = new[]
            {
            new { restaurantId = 1, menuItemId = 1, quantity = 2, price = 50.00m }
        },
            cardNumber = "4111111111111111",
            expiryDate = "12/25",
            cvv = "123",
            customerEmail = "test@example.com"
        };

        var createResponse = await _client.PostAsync(
            "/order/create",
            new StringContent(JsonConvert.SerializeObject(orderPayload), Encoding.UTF8, "application/json")
        );

        createResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Order creation should succeed");

        // Extract order ID
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createResult = JsonConvert.DeserializeObject<dynamic>(createResponseContent);
        int orderId = createResult.result != null ? (int)createResult.result : -1;
        orderId.Should().BeGreaterThan(0, "Order ID should be returned after creation");

        Console.WriteLine($"Created Order ID: {orderId}");

        // Step 2: Update order status
        int newStatusId = 2; // Example: Updating to a valid status ID
        var updateResponse = await _client.PutAsJsonAsync($"/order/updateStatus/{orderId}", newStatusId);

        // Log response for debugging
        string updateResponseContent = await updateResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Update Response: {updateResponseContent}");

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Updating the order status should succeed");

        // Step 3: Deserialize and validate response
        var updateResult = JsonConvert.DeserializeObject<dynamic>(updateResponseContent);
        bool isSuccess = updateResult.isSuccess != null ? (bool)updateResult.isSuccess : false;

        isSuccess.Should().BeTrue("Response should indicate success");
        ((string)updateResult.message).Should().Be("Order status updated successfully.", "The response message should match");
    }




    [Fact]
    public async Task CreateOrder_InvalidData_ShouldReturnBadRequest() {
        // Arrange
        var invalidOrderPayload = new {
            userId = "", // Missing required fields
            correlationId = Guid.NewGuid(),
            totalAmount = 0,
            items = new object[0],
            cardNumber = "",
            expiryDate = "",
            cvv = "",
            customerEmail = ""
        };

        // Act
        var response = await _client.PostAsync(
            "/order/create",
            new StringContent(JsonConvert.SerializeObject(invalidOrderPayload), Encoding.UTF8, "application/json")
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "Creating an order with invalid data should fail");
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

        Assert.False((bool)result.isSuccess, "Response should indicate failure");
    }

    [Fact]
    public async Task ConcurrentOrderCreation_ShouldSucceedForAll() {
        // Arrange
        var tasks = new List<Task<HttpResponseMessage>>();

        for (int i = 0; i < 5; i++) {
            var orderPayload = new {
                userId = $"concurrent_user_{i}",
                correlationId = Guid.NewGuid(),
                totalAmount = 100,
                items = new[] { new { RestaurantId = 1, MenuItemId = 1, Quantity = 1, Price = 20.00 } },
                cardNumber = "4111111111111111",
                expiryDate = "12/25",
                cvv = "123",
                customerEmail = $"test{i}@example.com"
            };

            tasks.Add(_client.PostAsync(
                "/order/create",
                new StringContent(JsonConvert.SerializeObject(orderPayload), Encoding.UTF8, "application/json")
            ));
        }

        // Act
        var responses = await Task.WhenAll(tasks);

        // Assert
        foreach (var response in responses) {
            response.StatusCode.Should().Be(HttpStatusCode.OK, "All concurrent orders should succeed");
        }
    }



}
