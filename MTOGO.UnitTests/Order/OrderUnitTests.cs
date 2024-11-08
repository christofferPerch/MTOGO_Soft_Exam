using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Moq;
using Xunit;
using MTOGO.Services.OrderAPI.Models;
using MTOGO.Services.OrderAPI.Models.Dto;
using MTOGO.Services.OrderAPI.Services;
using MTOGO.Services.OrderAPI.Services.IServices;
using Microsoft.Extensions.Logging;
using MTOGO.MessageBus;
using Microsoft.Extensions.Configuration;
using MTOGO.Services.DataAccess;

namespace MTOGO.UnitTests.Order
{
    public class OrderServiceTests
    {
        private readonly Mock<IDataAccess> _dataAccessMock;
        private readonly Mock<IMessageBus> _messageBusMock;
        private readonly ILogger<OrderService> _logger;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _dataAccessMock = new Mock<IDataAccess>();
            _messageBusMock = new Mock<IMessageBus>();
            _logger = new LoggerFactory().CreateLogger<OrderService>();
            var configurationMock = new Mock<IConfiguration>();
            _orderService = new OrderService(_dataAccessMock.Object, _logger, _messageBusMock.Object, configurationMock.Object);
        }

       /* [Fact]
        public async Task ProcessPayment_ShouldPublishPaymentSuccessMessage_WhenPaymentIsValid()
        {
            // Arrange
            var paymentRequest = new PaymentRequestDto
            {
                UserId = "testUser",
                CorrelationId = Guid.NewGuid(),
                CardNumber = "1234567890123456",
                ExpiryDate = "12/24",
                CVV = "123"
            };

            var cartResponse = new CartResponseMessageDto
            {
                UserId = paymentRequest.UserId,
                CorrelationId = paymentRequest.CorrelationId,
                Items = new List<CartItem>
        {
            new CartItem { RestaurantId = 1, MenuItemId = 1, Quantity = 2, Price = 10 }
        }
            };

            // Mock GetCartDetails to return a valid cart response
            //_orderService.GetCartDetails = (string userId, Guid correlationId) => Task.FromResult<CartResponseMessageDto>(cartResponse);

            // Mock PublishMessage to verify it's called
            _messageBusMock.Setup(mb => mb.PublishMessage("PaymentSuccessQueue", It.IsAny<string>()))
                           .Returns(Task.CompletedTask);

            // Act
            var paymentResponse = await _orderService.ProcessPayment(paymentRequest);

            // Assert
            Assert.True(paymentResponse.IsSuccessful);
            Assert.Equal("Payment processed successfully.", paymentResponse.Message);

            // Verify that PublishMessage was called once on the PaymentSuccessQueue
            _messageBusMock.Verify(mb => mb.PublishMessage("PaymentSuccessQueue", It.IsAny<string>()), Times.Once);
        }*/


        [Fact]
        public async Task CreateOrder_ShouldReturn_NewOrderId()
        {
            // Arrange
            var addOrderDto = new AddOrderDto
            {
                UserId = "testUser",
                TotalAmount = 100,
                VATAmount = 25,
                Items = new List<CartItem>
                {
                    new CartItem { RestaurantId = 1, MenuItemId = 1, Price = 10, Quantity = 2 }
                }
            };

            _dataAccessMock.Setup(d => d.ExecuteStoredProcedure<int>("AddOrder", It.IsAny<object>()))
                           .Callback<string, object>((procName, dynamicParams) =>
                           {
                               if (dynamicParams is DynamicParameters parameters)
                               {
                                   parameters.Add("@OrderId", 1, DbType.Int32, ParameterDirection.Output);
                               }
                           })
                           .ReturnsAsync(1);

            // Act
            var orderId = await _orderService.CreateOrder(addOrderDto);

            // Assert
            Assert.Equal(1, orderId);
            _dataAccessMock.Verify(d => d.ExecuteStoredProcedure<int>("AddOrder", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task PublishCartRemovalMessage_ShouldPublishMessage_WhenCalled()
        {
            // Arrange
            var userId = "testUser";

            _messageBusMock.Setup(mb => mb.PublishMessage("CartRemovedQueue", It.IsAny<string>()))
                           .Returns(Task.CompletedTask);

            // Act
            await _orderService.PublishCartRemovalMessage(userId);

            // Assert
            _messageBusMock.Verify(mb => mb.PublishMessage("CartRemovedQueue", It.Is<string>(message => message.Contains(userId))), Times.Once);
        }
    }
}
