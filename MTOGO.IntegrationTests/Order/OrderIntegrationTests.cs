using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MTOGO.IntegrationTests.Mocks;
using MTOGO.Services.OrderAPI.Models;
using MTOGO.Services.OrderAPI.Models.Dto;


namespace MTOGO.IntegrationTests.Order
{
    public class OrderIntegrationTests : IClassFixture<CustomOrderWebApplicationFactory<MTOGO.Services.OrderAPI.Program>>
    {
        private readonly HttpClient _client;

        public OrderIntegrationTests(CustomOrderWebApplicationFactory<MTOGO.Services.OrderAPI.Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetOrderHistory_ShouldReturnOrders()
        {
            var response = await _client.GetAsync("api/order/order-history/TestUser");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();

            var orders = JsonSerializer.Deserialize<List<OrderDto>>(
                responseData.Result!.ToString(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            orders.Should().NotBeNull();
            orders!.Should().NotBeEmpty("Order history should return records.");
        }

        [Fact]
        public async Task GetActiveOrders_ShouldReturnOrders()
        {
            var response = await _client.GetAsync("api/order/active-orders/TestUser");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();

            var orders = JsonSerializer.Deserialize<List<OrderDto>>(
                responseData.Result!.ToString(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            orders.Should().NotBeNull();
            orders!.Should().NotBeEmpty("Active orders should return records.");
        }

        [Fact]
        public async Task GetOrderById_ShouldReturnOrder()
        {
            var response = await _client.GetAsync("api/order/1");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();

            var order = JsonSerializer.Deserialize<OrderDto>(
                responseData.Result!.ToString(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            order.Should().NotBeNull("Order should exist with the given ID.");
        }

        [Fact]
        public async Task UpdateOrderStatus_ShouldReturnOk()
        {
            var newStatusId = 2;

            var response = await _client.PutAsJsonAsync("api/order/updateStatus/1", newStatusId);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();
            responseData.Message.Should().Be("Order status updated successfully.");
        }
    }
}
