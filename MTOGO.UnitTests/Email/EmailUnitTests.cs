using Moq;
using Xunit;
using MTOGO.Services.EmailAPI.Services;
using MTOGO.Services.EmailAPI.Models.Dto;
using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Threading.Tasks;

namespace MTOGO.UnitTests.Email
{
    public class EmailServiceUnitTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<RestClient> _restClientMock;

        public EmailServiceUnitTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _restClientMock = new Mock<RestClient>();
        }

        [Fact]
        public async Task SendOrderCreatedEmailAsync_InvalidOrder_ReturnsFalse()
        {
            var order = new OrderCreatedMessageDto
            {
                OrderId = 0, 
                CustomerEmail = "",
                TotalAmount = 0.0m,
                Items = new List<OrderItemDto>()
            };

            var mockResponse = new RestResponse { StatusCode = System.Net.HttpStatusCode.BadRequest };

            var restClientMock = new Mock<IRestClient>();
            restClientMock
                .Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            var emailService = new EmailService(_configurationMock.Object);
            var result = await emailService.SendOrderCreatedEmailAsync(order);

            Assert.False(result);
        }

        [Fact]
        public async Task SendOrderCreatedEmailAsync_ThrowsException_ReturnsFalse()
        {
            var order = new OrderCreatedMessageDto
            {
                OrderId = 123,
                CustomerEmail = "customer@test.com",
                TotalAmount = 50.0m,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { Name = "Pizza", Quantity = 2, Price = 20.0m },
                    new OrderItemDto { Name = "Pasta", Quantity = 1, Price = 10.0m }
                }
            };

            var restClientMock = new Mock<IRestClient>();
            restClientMock
                .Setup(c => c.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Failed to send email"));

            var emailService = new EmailService(_configurationMock.Object);
            var result = await emailService.SendOrderCreatedEmailAsync(order);

            Assert.False(result);
        }
    }
}