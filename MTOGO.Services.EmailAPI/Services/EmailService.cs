using MTOGO.Services.EmailAPI.Models.Dto;
using MTOGO.Services.EmailAPI.Services.IServices;
using RestSharp;
using RestSharp.Authenticators;

namespace MTOGO.Services.EmailAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendOrderCreatedEmailAsync(OrderCreatedMessageDto order)
        {
            try
            {
                var testKey = Environment.GetEnvironmentVariable("MAILGUN_API_KEY");
                var domain = Environment.GetEnvironmentVariable("MAILGUN_DOMAIN");
                var fromEmail = Environment.GetEnvironmentVariable("MAILGUN_FROM_EMAIL");

                var client = new RestClient(new RestClientOptions
                {
                    BaseUrl = new Uri($"https://api.mailgun.net/v3/{domain}"),
                    Authenticator = new HttpBasicAuthenticator("api", testKey)
                });

                var request = new RestRequest("messages", Method.Post);
                request.AddParameter("from", fromEmail);
                request.AddParameter("to", order.CustomerEmail);
                request.AddParameter("subject", $"Order Confirmation - Order #{order.OrderId}");
                request.AddParameter("html", GenerateEmailBody(order));

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {            
                return false;
                throw;
            }
        }

        private string GenerateEmailBody(OrderCreatedMessageDto order)
        {
            var itemsHtml = string.Join("", order.Items.Select(i =>
                $"<li>{i.Quantity}x {i.Name} - ${i.Price * i.Quantity}</li>"));

            return $@"
                <h2>Thank you for your order!</h2>
                <p>Order ID: {order.OrderId}</p>
                <ul>{itemsHtml}</ul>
                <p>Total: ${order.TotalAmount}</p>
            ";
        }
    }
}
