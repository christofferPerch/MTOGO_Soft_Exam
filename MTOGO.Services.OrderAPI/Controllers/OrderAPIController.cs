using Microsoft.AspNetCore.Mvc;
using MTOGO.MessageBus;
using MTOGO.Services.OrderAPI.Models.Dto;
using MTOGO.Services.OrderAPI.Services.IServices;

namespace MTOGO.Services.OrderAPI.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderAPIController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<OrderAPIController> _logger;
        protected ResponseDto _response;

        public OrderAPIController(IOrderService orderService, IMessageBus messageBus, ILogger<OrderAPIController> logger)
        {
            _orderService = orderService;
            _messageBus = messageBus;
            _logger = logger;
            _response = new();
        }

        [HttpPost("processPayment")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto paymentRequest)
        {
            if (paymentRequest == null)
            {
                return BadRequest(new ResponseDto { IsSuccess = false, Message = "Invalid payment data." });
            }

            var paymentResponse = await _orderService.ProcessPayment(paymentRequest);
            return Ok(new ResponseDto { Result = paymentResponse, IsSuccess = paymentResponse.IsSuccessful, Message = paymentResponse.Message });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] AddOrderDto orderDto) {
            try {
                if (orderDto == null || orderDto.Items == null || !orderDto.Items.Any()) {
                    _response.IsSuccess = false;
                    _response.Message = "Invalid order data.";
                    return BadRequest(_response);
                }

                var orderId = await _orderService.CreateOrder(orderDto);

                if (orderId > 0) {
                    _response.Result = orderId;
                    _response.IsSuccess = true;
                    _response.Message = $"Order created successfully with ID: {orderId}";
                    return Ok(_response);
                }

                _response.IsSuccess = false;
                _response.Message = "Failed to create order.";
                return BadRequest(_response);
            } catch (Exception ex) {
                _logger.LogError(ex, "Error creating order.");
                _response.IsSuccess = false;
                _response.Message = "An error occurred while creating the order.";
                return StatusCode(500, _response);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderById(id);
                if (order == null)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Order not found.";
                    return NotFound(_response);
                }

                _response.Result = order;
                _response.Message = "Order retrieved successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order.");
                _response.IsSuccess = false;
                _response.Message = "An error occurred while retrieving the order.";
                return StatusCode(500, _response);
            }
        }

        [HttpPut("updateStatus/{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] int statusId)
        {
            try
            {
                var success = await _orderService.UpdateOrderStatus(orderId, statusId);
                if (success == 0)
                {
                    _response.IsSuccess = false;
                    _response.Message = "Order not found.";
                    return NotFound(_response);
                }

                _response.Message = "Order status updated successfully.";
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status.");
                _response.IsSuccess = false;
                _response.Message = "An error occurred while updating order status.";
                return StatusCode(500, _response);
            }
        }
    }
}
