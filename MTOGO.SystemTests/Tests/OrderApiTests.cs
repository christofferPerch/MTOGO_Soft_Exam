using System.Text;
using Newtonsoft.Json;

public class OrderApiSystemTests
{

    private readonly HttpClient _client;

    public OrderApiSystemTests()
    {
        _client = new HttpClient { BaseAddress = new Uri("http://localhost:7777") };
    }

    [Fact]
    public async Task GetOrderById_EndToEnd_ShouldReturnSuccessResponse()
    {
        var orderPayload = new
        {
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

        var getOrderResponse = await _client.GetAsync($"/order/{1}");

        Assert.True(getOrderResponse.IsSuccessStatusCode, "Retrieving order failed");
    }
}
