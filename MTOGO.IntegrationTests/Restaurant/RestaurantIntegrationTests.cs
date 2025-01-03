using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Data.SqlClient;
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
                },
                FoodCategories = new List<FoodCategoryDto>
                {
                    new FoodCategoryDto { Category = Category.Pizza },
                    new FoodCategoryDto { Category = Category.Burger }
                },
                        OperatingHours = new List<OperatingHoursDto>
                {
                    new OperatingHoursDto { Day = DayEnum.Monday, OpeningHours = new TimeSpan(9, 0, 0), ClosingHours = new TimeSpan(21, 0, 0) },
                    new OperatingHoursDto { Day = DayEnum.Tuesday, OpeningHours = new TimeSpan(9, 0, 0), ClosingHours = new TimeSpan(21, 0, 0) },
                    new OperatingHoursDto { Day = DayEnum.Wednesday, OpeningHours = new TimeSpan(9, 0, 0), ClosingHours = new TimeSpan(21, 0, 0) }
                }
                };

            var response = await _client.PostAsJsonAsync("api/restaurant/AddRestaurant", newRestaurant);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();

            var restaurantId = JsonSerializer.Deserialize<int>(
                responseData.Result!.ToString(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            restaurantId.Should().BeGreaterThan(0, "The restaurant ID should be greater than zero.");
        }

        [Fact]
        public async Task GetAllRestaurants_ShouldReturnRestaurants()
        {
            var response = await _client.GetAsync("api/restaurant/AllRestaurants");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();

            var restaurants = JsonSerializer.Deserialize<List<RestaurantDto>>(
                responseData.Result!.ToString(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            restaurants.Should().NotBeNull("The Result field should contain a list of restaurants.");
            restaurants!.Should().NotBeEmpty("The restaurants list should not be empty.");
        }

        [Fact]
        public async Task UpdateRestaurant_ValidData_ShouldReturnOk()
        {
            var updateRestaurant = new UpdateRestaurantDto
            {
                Id = 1,
                RestaurantName = "Updated Restaurant",
                LegalName = "Updated Legal Name",
                VATNumber = "VAT789",
                RestaurantDescription = "Updated test restaurant",
                ContactEmail = "updatedtest@example.com",
                ContactPhone = "123-456-7899",
                Address = new AddressDto
                {
                    AddressLine1 = "789 Updated St",
                    AddressLine2 = "Suite 202",
                    City = "Updated City",
                    ZipCode = "54321",
                    Country = "Updated Country"
                }
            };

            var response = await _client.PutAsJsonAsync("api/restaurant/UpdateRestaurant", updateRestaurant);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();

        }

        [Fact]
        public async Task RemoveRestaurant_ShouldReturnOk()
        {
            var response = await _client.DeleteAsync("api/restaurant/RemoveRestaurant?id=1");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();

        }

        [Fact]
        public async Task AddMenuItem_ShouldReturnOk()
        {
            var newMenuItem = new AddMenuItemDto
            {
                RestaurantId = 1,
                Name = "New Menu Item",
                Description = "A test menu item",
                Price = 15.99m
            };

            var response = await _client.PostAsJsonAsync("api/restaurant/AddMenuItem", newMenuItem);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();

            var menuItemId = JsonSerializer.Deserialize<int>(
                responseData.Result!.ToString(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            menuItemId.Should().BeGreaterThan(0, "The menu item ID should be greater than zero.");
        }

        [Fact]
        public async Task RemoveMenuItem_ShouldReturnOk()
        {
            var response = await _client.DeleteAsync("api/restaurant/RemoveMenuItem?restaurantId=1&menuItemId=1");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task GetUniqueCities_ShouldReturnCities()
        {
            var response = await _client.GetAsync("api/restaurant/UniqueCities");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseData = await response.Content.ReadFromJsonAsync<ResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.IsSuccess.Should().BeTrue();

            var cities = JsonSerializer.Deserialize<List<string>>(
                responseData.Result!.ToString(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            cities.Should().NotBeNull();
            cities!.Should().NotBeEmpty("The response should contain a list of unique cities.");
        }

    }
}
