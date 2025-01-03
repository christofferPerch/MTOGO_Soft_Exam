﻿using Dapper;
using MTOGO.MessageBus;
using MTOGO.Services.DataAccess;
using MTOGO.Services.OrderAPI.Models;
using MTOGO.Services.OrderAPI.Models.Dto;
using MTOGO.Services.OrderAPI.Services.IServices;
using Newtonsoft.Json;
using System.Data;

namespace MTOGO.Services.OrderAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly IDataAccess _dataAccess;
        private readonly ILogger<OrderService> _logger;
        private readonly IMessageBus _messageBus;
        private readonly IConfiguration _configuration;


        public OrderService(IDataAccess dataAccess, ILogger<OrderService> logger, IMessageBus messageBus, IConfiguration configuration)
        {
            _dataAccess = dataAccess;
            _logger = logger;
            _messageBus = messageBus;
            _configuration = configuration;

            SubscribeToPaymentSuccessQueue();
        }

        #region Subscribe Methods
        private void SubscribeToPaymentSuccessQueue()
        {
            _messageBus.SubscribeMessage<PaymentRequestDto>("PaymentSuccessQueue", async paymentRequest =>
            {
                if (paymentRequest == null)
                {
                    _logger.LogError("Failed to deserialize PaymentRequestDto from PaymentSuccessQueue message.");
                    return;
                }

                var order = new AddOrderDto
                {
                    UserId = paymentRequest.UserId,
                    TotalAmount = paymentRequest.TotalAmount,
                    VATAmount = paymentRequest.TotalAmount * 0.25m,
                    Items = paymentRequest.Items,
                    CustomerEmail = paymentRequest.CustomerEmail
                };

                try
                {
                    int orderId = await CreateOrder(order);
                    _logger.LogInformation($"Order created successfully with Order ID: {orderId}");

                    await PublishCartRemovalMessage(paymentRequest.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating order from payment success.");
                }
            });
        }


        #endregion

        #region Payment Methods
        public async Task<PaymentResponseDto> ProcessPayment(PaymentRequestDto paymentRequest)
        {

            var cartDetails = await GetCartDetails(paymentRequest.UserId, paymentRequest.CorrelationId);

            if (cartDetails == null)
            {
                return new PaymentResponseDto
                {
                    UserId = paymentRequest.UserId,
                    CorrelationId = paymentRequest.CorrelationId,
                    IsSuccessful = false,
                    Message = "Failed to retrieve shopping cart details."
                };
            }

            paymentRequest.Items = cartDetails.Items;
            paymentRequest.TotalAmount = cartDetails.Items.Sum(item => item.Price * item.Quantity);

            bool isPaymentValid = ValidatePaymentDetails(paymentRequest);

            var paymentResponse = new PaymentResponseDto
            {
                UserId = paymentRequest.UserId,
                CorrelationId = paymentRequest.CorrelationId,
                IsSuccessful = isPaymentValid,
                Message = isPaymentValid ? "Payment processed successfully." : "Payment failed."
            };

            if (isPaymentValid)
            {
                await _messageBus.PublishMessage("PaymentSuccessQueue", JsonConvert.SerializeObject(paymentRequest));
                _logger.LogInformation($"Payment success published for User ID: {paymentRequest.UserId}");
            }

            return paymentResponse;
        }

        public async Task<CartResponseMessageDto?> GetCartDetails(string userId, Guid correlationId)
        {

            var cartRequest = new CartRequestMessageDto
            {
                UserId = userId,
                CorrelationId = correlationId,
            };

            string cartRequestQueue = "CartRequestQueue";
            await _messageBus.PublishMessage(cartRequestQueue, JsonConvert.SerializeObject(cartRequest));

            var tcs = new TaskCompletionSource<CartResponseMessageDto>();

            _messageBus.SubscribeMessage<CartResponseMessageDto>("CartResponseQueue", message =>
            {
                if (message.CorrelationId == correlationId)
                {
                    tcs.SetResult(message);
                }
            });

            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(30)));
            if (completedTask == tcs.Task)
            {
                var result = tcs.Task.Result;

                return result;
            }
            else
            {
                _logger.LogWarning("Timeout waiting for cart response.");
                return null;
            }
        }


        private bool ValidatePaymentDetails(PaymentRequestDto paymentRequest)
        {
            return !string.IsNullOrEmpty(paymentRequest.CardNumber) &&
                   !string.IsNullOrEmpty(paymentRequest.ExpiryDate) &&
                   !string.IsNullOrEmpty(paymentRequest.CVV);
        }
        #endregion

        #region Order Methods
        public async Task<int> CreateOrder(AddOrderDto order)
        {
            try
            {
                var orderItemsTable = new DataTable();
                orderItemsTable.Columns.Add("RestaurantId", typeof(int));
                orderItemsTable.Columns.Add("MenuItemId", typeof(int));
                orderItemsTable.Columns.Add("Price", typeof(decimal));
                orderItemsTable.Columns.Add("Quantity", typeof(int));

                foreach (var item in order.Items)
                {
                    orderItemsTable.Rows.Add(
                        item.RestaurantId,
                        item.MenuItemId,
                        item.Price,
                        item.Quantity
                    );
                }

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", order.UserId);
                parameters.Add("@TotalAmount", order.TotalAmount);
                parameters.Add("@VATAmount", order.VATAmount);
                parameters.Add("@OrderPlacedTimestamp", DateTime.UtcNow);
                parameters.Add("@OrderStatusId", (int)OrderStatus.FreeToTake);
                parameters.Add("@OrderItems", orderItemsTable.AsTableValuedParameter("TVP_OrderItem"));
                parameters.Add("@OrderId", dbType: DbType.Int32, direction: ParameterDirection.Output);

                await _dataAccess.ExecuteStoredProcedure<int>("AddOrder", parameters);

                int orderId = parameters.Get<int>("@OrderId");

                // Map to OrderCreatedMessageDto
                var orderCreatedMessage = new OrderCreatedMessageDto
                {
                    OrderId = orderId,
                    UserId = order.UserId,
                    CustomerEmail = order.CustomerEmail, // Include CustomerEmail
                    TotalAmount = order.TotalAmount,
                    Items = order.Items.Select(i => new OrderItemDto
                    {
                        RestaurantId = i.RestaurantId,
                        MenuItemId = i.MenuItemId,
                        Quantity = i.Quantity,
                        Price = i.Price
                    }).ToList()
                };

                await _messageBus.PublishMessage("OrderCreatedQueue", JsonConvert.SerializeObject(orderCreatedMessage));
                _logger.LogInformation($"Order created event published for Order ID: {orderId}");

                return orderId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order.");
                throw;
            }
        }




        private async Task PublishOrderCreatedMessage(AddOrderDto order, int orderId)
        {
            try
            {
                var orderCreatedMessage = new OrderCreatedMessageDto
                {
                    OrderId = orderId,
                    UserId = order.UserId,
                    CustomerEmail = order.CustomerEmail,
                    TotalAmount = order.TotalAmount,
                    Items = order.Items.Select(i => new OrderItemDto
                    {
                        RestaurantId = i.RestaurantId,
                        MenuItemId = i.MenuItemId,
                        Price = i.Price,
                        Quantity = i.Quantity
                    }).ToList()
                };

                string message = JsonConvert.SerializeObject(orderCreatedMessage);
                await _messageBus.PublishMessage("OrderCreatedQueue", message);
                _logger.LogInformation($"OrderCreated message published for Order ID: {orderId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to publish OrderCreated message for Order ID: {orderId}");
            }
        }


        private async Task PublishCartRemovalMessage(string userId)
        {
            try
            {
                var cartRemovedMessage = new CartRemovedMessageDto { UserId = userId };
                await _messageBus.PublishMessage("CartRemovedQueue", JsonConvert.SerializeObject(cartRemovedMessage));
                _logger.LogInformation("Published cart removal message for user: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to publish cart removal message for user {userId}.");
            }
        }

        public async Task<OrderDto?> GetOrderById(int id)
        {
            try
            {
                var orderSql = "SELECT * FROM [Order] WHERE Id = @Id;";
                var order = await _dataAccess.GetById<OrderDto>(orderSql, new { Id = id });

                if (order == null)
                {
                    return null;
                }

                var orderItemsSql = "SELECT * FROM OrderItem WHERE OrderId = @OrderId;";
                var orderItems = await _dataAccess.GetAll<OrderItemDto>(orderItemsSql, new { OrderId = id });
                order.Items = orderItems;

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving order with ID {id}");
                throw;
            }
        }

        public async Task<int> UpdateOrderStatus(int orderId, int statusId)
        {
            try
            {
                var sql = "UPDATE [Order] SET OrderStatusId = @StatusId WHERE Id = @OrderId;";
                var result = await _dataAccess.Update(sql, new { OrderId = orderId, StatusId = statusId });

                if (result > 0)
                {
                    var statusUpdateMessage = new OrderStatusUpdateDto
                    {
                        OrderId = orderId,
                        StatusId = statusId
                    };

                    await PublishOrderStatusUpdateMessage(statusUpdateMessage);
                }
                else
                {
                    _logger.LogWarning($"Order with ID {orderId} not found, status update was not applied.");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order status for ID {orderId}");
                throw;
            }
        }

        private async Task PublishOrderStatusUpdateMessage(OrderStatusUpdateDto statusUpdateMessage)
        {
            try
            {
                string message = JsonConvert.SerializeObject(statusUpdateMessage);
                await _messageBus.PublishMessage("OrderStatusQueue", message);
                _logger.LogInformation($"Order status update published for Order ID: {statusUpdateMessage.OrderId}, Status ID: {statusUpdateMessage.StatusId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to publish order status update for Order ID: {statusUpdateMessage.OrderId}");
            }
        }

        public async Task<List<OrderManagementDto>> GetActiveOrders(string userId)
        {
            try
            {
                var ordersSql = @"
                        SELECT * 
                        FROM [Order] 
                        WHERE UserId = @UserId AND OrderStatusId IN (0, 1, 2)";

                var orders = await _dataAccess.GetAll<OrderManagementDto>(ordersSql, new { UserId = userId }) ?? new List<OrderManagementDto>();

                foreach (var order in orders)
                {
                    var itemsSql = "SELECT * FROM [OrderItem] WHERE OrderId = @OrderId";
                    order.Items = await _dataAccess.GetAll<OrderItemDto>(itemsSql, new { OrderId = order.Id }) ?? new List<OrderItemDto>();
                }

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active orders for user {UserId}", userId);
                throw;
            }
        }


        public async Task<List<OrderManagementDto>> GetOrderHistory(string userId)
        {
            try
            {
                // Query to fetch orders with status 3
                var ordersSql = @"
            SELECT * 
            FROM [Order] 
            WHERE UserId = @UserId AND OrderStatusId = 3";

                var orders = await _dataAccess.GetAll<OrderManagementDto>(ordersSql, new { UserId = userId }) ?? new List<OrderManagementDto>();

                // Fetch order items for each order
                foreach (var order in orders)
                {
                    var itemsSql = "SELECT * FROM [OrderItem] WHERE OrderId = @OrderId";
                    order.Items = await _dataAccess.GetAll<OrderItemDto>(itemsSql, new { OrderId = order.Id }) ?? new List<OrderItemDto>();
                }

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order history for user {UserId}", userId);
                throw;
            }
        }

        #endregion

    }
}
