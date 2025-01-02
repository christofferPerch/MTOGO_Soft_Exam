using System.Net.Http.Json;
using FluentAssertions;
using MTOGO.Services.RestaurantAPI.Models.Dto;


namespace MTOGO.IntegrationTests.Restaurant
{
    public class RestaurantIntegrationTests : IClassFixture<CustomRestaurantWebApplicationFactory<MTOGO.Services.RestaurantAPI.Program>>
    {
        private readonly HttpClient _client;

        public RestaurantIntegrationTests(CustomRestaurantWebApplicationFactory<MTOGO.Services.RestaurantAPI.Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetRestaurantById_ShouldReturnRestaurant()
        {
            var response = await _client.GetAsync("api/restaurant/GetRestaurantBy/1");

            if (!response.IsSuccessStatusCode)
            {
                var errorDetails = await response.Content.ReadAsStringAsync();
                throw new Exception($"Unexpected response status: {response.StatusCode}, Details: {errorDetails}");
            }

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var data = await response.Content.ReadFromJsonAsync<ResponseDto>();
            data.Should().NotBeNull();
            data!.Result.Should().NotBeNull();
        }


        [Fact]
        public async Task AddRestaurant_ValidData_ShouldReturnOk()
        {
            var newRestaurant = new AddRestaurantDto
            {
                RestaurantName = "New Restaurant",
                LegalName = "New Legal Name",
                VATNumber = "VAT456",
                RestaurantDescription = "A new test restaurant",
                ContactEmail = "newtest@example.com",
                ContactPhone = "987-654-3210",
                Address = new AddressDto
                {
                    AddressLine1 = "456 Another St",
                    AddressLine2 = "Suite 101",
                    City = "Another City",
                    ZipCode = "67890",
                    Country = "Another Country"
                }
            };

            var response = await _client.PostAsJsonAsync("api/restaurant/AddRestaurant", newRestaurant);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var data = await response.Content.ReadFromJsonAsync<ResponseDto>();
            data.Should().NotBeNull();
            data!.Result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllRestaurants_ShouldReturnRestaurants()
        {
            var response = await _client.GetAsync("api/restaurant/AllRestaurants");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            var data = await response.Content.ReadFromJsonAsync<ResponseDto>();
            data.Should().NotBeNull();
            data!.Result.Should().BeOfType<List<RestaurantDto>>();
        }
    }
}
