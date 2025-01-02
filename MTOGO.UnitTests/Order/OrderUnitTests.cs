using Moq;
using Xunit;
using MTOGO.MessageBus;
using MTOGO.Services.DataAccess;
using MTOGO.Services.OrderAPI.Models;
using MTOGO.Services.OrderAPI.Models.Dto;
using MTOGO.Services.OrderAPI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Data;
using Dapper;

namespace MTOGO.UnitTests.Order
{
    public class OrderUnitTests
    {
        private readonly Mock<IDataAccess> _dataAccessMock;
        private readonly Mock<ILogger<OrderService>> _loggerMock;
        private readonly Mock<IMessageBus> _messageBusMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly OrderService _orderService;

        public OrderUnitTests()
        {
            _dataAccessMock = new Mock<IDataAccess>();
            _loggerMock = new Mock<ILogger<OrderService>>();
            _messageBusMock = new Mock<IMessageBus>();
            _configurationMock = new Mock<IConfiguration>();
            _orderService = new OrderService(
                _dataAccessMock.Object,
                _loggerMock.Object,
                _messageBusMock.Object,
                _configurationMock.Object
            );
        }

        [Fact]
        public async Task CreateOrder_ValidOrder_ReturnsOrderId()
        {
            var addOrderDto = new AddOrderDto
            {
                UserId = "user123",
                TotalAmount = 100,
                VATAmount = 25,
                Items = new List<CartItem>
                {
                    new CartItem { RestaurantId = 1, MenuItemId = 101, Price = 50, Quantity = 2 }
                },
                CustomerEmail = "test@example.com"
            };

            _dataAccessMock.Setup(d => d.ExecuteStoredProcedure<int>("AddOrder", It.IsAny<object>()))
                           .Callback<string, object>((_, parameters) =>
                           {
                               if (parameters is DynamicParameters dynamicParams)
                               {
                                   dynamicParams.Add("@OrderId", 1, DbType.Int32, ParameterDirection.Output);
                               }
                           });

            var result = await _orderService.CreateOrder(addOrderDto);

            Assert.Equal(1, result);
            _dataAccessMock.Verify(d => d.ExecuteStoredProcedure<int>("AddOrder", It.IsAny<object>()), Times.Once);
            _messageBusMock.Verify(m => m.PublishMessage("OrderCreatedQueue", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessPayment_ValidPayment_ReturnsSuccessfulResponse()
        {
            var correlationId = Guid.NewGuid();
            var paymentRequest = new PaymentRequestDto
            {
                UserId = "user123",
                CorrelationId = correlationId,
                CardNumber = "1234567890123456",
                ExpiryDate = "12/24",
                CVV = "123"
            };

            var cartResponse = new CartResponseMessageDto
            {
                UserId = "user123",
                CorrelationId = correlationId,
                Items = new List<CartItem>
                {
                    new CartItem { MenuItemId = 101, RestaurantId = 1, Price = 50.0m, Quantity = 2 }
                }
            };

            _messageBusMock
                .Setup(m => m.PublishMessage("CartRequestQueue", It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _messageBusMock
                .Setup(m => m.SubscribeMessage<CartResponseMessageDto>("CartResponseQueue", It.IsAny<Action<CartResponseMessageDto>>()))
                .Callback<string, Action<CartResponseMessageDto>>((queue, callback) =>
                {
                    if (queue == "CartResponseQueue")
                    {
                        callback(cartResponse); 
                    }
                });

            _messageBusMock
                .Setup(m => m.PublishMessage("PaymentSuccessQueue", It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var result = await _orderService.ProcessPayment(paymentRequest);

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.Equal("Payment processed successfully.", result.Message);
            _messageBusMock.Verify(m => m.PublishMessage("CartRequestQueue", It.IsAny<string>()), Times.Once);
            _messageBusMock.Verify(m => m.PublishMessage("PaymentSuccessQueue", It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public async Task GetOrderById_ValidId_ReturnsOrder()
        {
            var orderId = 1;
            var expectedOrder = new OrderDto
            {
                UserId = "user123",
                TotalAmount = 100,
                VATAmount = 25,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { RestaurantId = 1, MenuItemId = 101, Price = 50, Quantity = 2 }
                }
            };

            _dataAccessMock.Setup(d => d.GetById<OrderDto>(It.IsAny<string>(), It.IsAny<object>()))
                           .ReturnsAsync(expectedOrder);
            _dataAccessMock.Setup(d => d.GetAll<OrderItemDto>(It.IsAny<string>(), It.IsAny<object>()))
                           .ReturnsAsync(expectedOrder.Items);

            var result = await _orderService.GetOrderById(orderId);

            Assert.NotNull(result);
            Assert.Equal("user123", result.UserId);
            Assert.Equal("user123", result.UserId);
            _dataAccessMock.Verify(d => d.GetById<OrderDto>(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            _dataAccessMock.Verify(d => d.GetAll<OrderItemDto>(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task UpdateOrderStatus_ValidId_UpdatesStatusAndPublishesMessage()
        {
            var orderId = 1;
            var statusId = 2;

            _dataAccessMock.Setup(d => d.Update(It.IsAny<string>(), It.IsAny<object>()))
                           .ReturnsAsync(1);

            var result = await _orderService.UpdateOrderStatus(orderId, statusId);

            Assert.Equal(1, result);
            _dataAccessMock.Verify(d => d.Update(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            _messageBusMock.Verify(m => m.PublishMessage("OrderStatusQueue", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetActiveOrders_ValidUserId_ReturnsOrders()
        {
            var userId = "user123";
            var expectedOrders = new List<OrderManagementDto>
            {
                new OrderManagementDto
                {
                    Id = 1,
                    UserId = userId,
                    TotalAmount = 100,
                    Items = new List<OrderItemDto>
                    {
                        new OrderItemDto { RestaurantId = 1, MenuItemId = 101, Price = 50, Quantity = 2 }
                    }
                }
            };

            _dataAccessMock.Setup(d => d.GetAll<OrderManagementDto>(It.IsAny<string>(), It.IsAny<object>()))
                           .ReturnsAsync(expectedOrders);

            var result = await _orderService.GetActiveOrders(userId);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(userId, result[0].UserId);
            _dataAccessMock.Verify(d => d.GetAll<OrderManagementDto>(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task GetOrderHistory_ValidUserId_ReturnsOrderHistory()
        {
            var userId = "user123";
            var expectedHistory = new List<OrderManagementDto>
            {
                new OrderManagementDto
                {
                    Id = 1,
                    UserId = userId,
                    TotalAmount = 100,
                    Items = new List<OrderItemDto>
                    {
                        new OrderItemDto { RestaurantId = 1, MenuItemId = 101, Price = 50, Quantity = 2 }
                    }
                }
            };

            _dataAccessMock.Setup(d => d.GetAll<OrderManagementDto>(It.IsAny<string>(), It.IsAny<object>()))
                           .ReturnsAsync(expectedHistory);

            var result = await _orderService.GetOrderHistory(userId);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(userId, result[0].UserId);
            _dataAccessMock.Verify(d => d.GetAll<OrderManagementDto>(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }
}