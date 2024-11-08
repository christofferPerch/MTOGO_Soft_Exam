using Dapper;
using MTOGO.MessageBus;
using MTOGO.Services.DataAccess;
using MTOGO.Services.OrderAPI.Models;
using MTOGO.Services.OrderAPI.Models.Dto;
using MTOGO.Services.OrderAPI.Services.IServices;
using System.Data;

namespace MTOGO.Services.OrderAPI.Services
{
    public class OrderService : IOrderService, IHostedService
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
                    Items = paymentRequest.Items
                };

                try
                {
                    int orderId = await CreateOrder(order);
                    _logger.LogInformation($"Order created successfully with Order ID: {orderId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating order from payment success.");
                }
            });
        }

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
                return orderId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order.");
                throw;
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
                return await _dataAccess.Update(sql, new { OrderId = orderId, StatusId = statusId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order status for ID {orderId}");
                throw;
            }
        }
    }
}
