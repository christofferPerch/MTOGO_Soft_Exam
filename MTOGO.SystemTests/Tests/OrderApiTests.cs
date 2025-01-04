using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xunit;

public class OrderApiSystemTests {
    private readonly HttpClient _client;

    public OrderApiSystemTests() {
        // Set up HttpClient with the Gateway URL
        _client = new HttpClient { BaseAddress = new Uri("http://localhost:7777") }; // Gateway URL
    }

    [Fact]
    public async Task GetOrderById_EndToEnd_ShouldReturnSuccessResponse() {
        // Step 1: Create an order
        var orderPayload = new {
            userId = "test-user",
            correlationId = Guid.NewGuid(),
            totalAmount = 100,
            items = new[] {
                new { restaurantId = 1, menuItemId = 1, quantity = 2, price = 50 }
            },
            cardNumber = "4111111111111111",
            expiryDate = "12/25",
            cvv = "123",
            customerEmail = "test@example.com"
        };

        var orderResponse = await _client.PostAsync(
            "/order/create",
            new StringContent(JsonConvert.SerializeObject(orderPayload), Encoding.UTF8, "application/json")
        );

        // Assert success for order creation
        Assert.True(orderResponse.IsSuccessStatusCode, "Order creation failed");

        // Step 2: Retrieve the created order
        var getOrderResponse = await _client.GetAsync($"/order/{1}"); // Use 1 as a placeholder for the order ID

        // Assert success for order retrieval
        Assert.True(getOrderResponse.IsSuccessStatusCode, "Retrieving order failed");
    }
}
