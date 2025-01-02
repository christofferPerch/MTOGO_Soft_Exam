using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using MTOGO.Services.OrderAPI.Services.IServices;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

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
    public async Task CustomerCanViewOrderHistory_ShouldReturnOrders() {
        // Step 1: Create an order
        var orderPayload = new {
            userId = "history_user",
            correlationId = Guid.NewGuid(),
            totalAmount = 120.00m, // Change to decimal
            items = new[]
     {
        new { restaurantId = 1, menuItemId = 1, quantity = 2, price = 50.00m }, // Change to decimal
        new { restaurantId = 1, menuItemId = 2, quantity = 1, price = 20.00m }  // Change to decimal
    },
            cardNumber = "4111111111111111",
            expiryDate = "12/25",
            cvv = "123",
            customerEmail = "history_user@example.com"
        };


        var orderResponse = await _client.PostAsync(
            "/order/create",
            new StringContent(JsonConvert.SerializeObject(orderPayload), Encoding.UTF8, "application/json")
        );

        Assert.True(orderResponse.IsSuccessStatusCode, "Order creation failed");

        // Step 2: Retrieve the order history for the user
        var historyResponse = await _client.GetAsync($"/order/order-history/history_user");

        Assert.True(historyResponse.IsSuccessStatusCode, "Retrieving order history failed");

        var historyResponseBody = await historyResponse.Content.ReadAsStringAsync();
        dynamic historyResult = JsonConvert.DeserializeObject(historyResponseBody);

        // Step 3: Validate the response
        Assert.NotNull(historyResult.result);
        Assert.NotEmpty(historyResult.result);

        var orderHistory = historyResult.result[0];
        Assert.Equal(orderPayload.userId, (string)orderHistory.userId);
        Assert.Equal(orderPayload.totalAmount, (decimal)orderHistory.totalAmount);
        Assert.NotEmpty(orderHistory.items);

        var firstItem = orderHistory.items[0];
        Assert.Equal(orderPayload.items[0].restaurantId, (int)firstItem.restaurantId);
        Assert.Equal(orderPayload.items[0].menuItemId, (int)firstItem.menuItemId);
        Assert.Equal(orderPayload.items[0].quantity, (int)firstItem.quantity);
        Assert.Equal(orderPayload.items[0].price, (decimal)firstItem.price);
    }

}
