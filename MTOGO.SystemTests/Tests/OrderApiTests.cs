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
    public async Task GetOrderById_EndToEnd_ShouldReturnOrderDetails() {
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

        Assert.True(orderResponse.IsSuccessStatusCode, "Order creation failed");

        var orderResponseBody = await orderResponse.Content.ReadAsStringAsync();
        dynamic orderResult = JsonConvert.DeserializeObject(orderResponseBody);

        // Ensure the "result" field is not null
        Assert.NotNull(orderResult.result);

        // Step 2: Retrieve the created order
        var getOrderResponse = await _client.GetAsync($"/order/{1}"); // Use 1 as a placeholder for the order ID

        Assert.True(getOrderResponse.IsSuccessStatusCode, "Retrieving order failed");

        var getOrderResponseBody = await getOrderResponse.Content.ReadAsStringAsync();
        dynamic getOrderResult = JsonConvert.DeserializeObject(getOrderResponseBody);

        // Step 3: Validate the order details
        Assert.Equal(orderPayload.userId, (string)getOrderResult.result.userId);
        Assert.Equal(orderPayload.totalAmount, (decimal)getOrderResult.result.totalAmount);
        Assert.NotEmpty(getOrderResult.result.items);

        var createdItem = getOrderResult.result.items[0];
        var originalItem = orderPayload.items[0];

        Assert.Equal(originalItem.restaurantId, (int)createdItem.restaurantId);
        Assert.Equal(originalItem.menuItemId, (int)createdItem.menuItemId);
        Assert.Equal(originalItem.quantity, (int)createdItem.quantity);
        Assert.Equal(originalItem.price, (decimal)createdItem.price);
    }
}
