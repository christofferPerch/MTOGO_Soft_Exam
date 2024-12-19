using Dapper;
using Microsoft.Extensions.Logging;
using Moq;
using MTOGO.Services.DataAccess;
using MTOGO.Services.RestaurantAPI.Models.Dto;
using MTOGO.Services.RestaurantAPI.Services;
using System.Data;

namespace MTOGO.UnitTests.Restaurant
{
    public class RestaurantServiceTests
    {
        private readonly Mock<IDataAccess> _dataAccessMock;
        private readonly ILogger<RestaurantService> _logger;
        private readonly RestaurantService _restaurantService;

        public RestaurantServiceTests()
        {
            _dataAccessMock = new Mock<IDataAccess>();
            _logger = new LoggerFactory().CreateLogger<RestaurantService>();
            _restaurantService = new RestaurantService(_dataAccessMock.Object, _logger);
        }

        [Fact]
        public async Task AddRestaurant_ShouldReturn_NewRestaurantId()
        {
            var addRestaurantDto = new AddRestaurantDto
            {
                RestaurantName = "Test Restaurant",
                LegalName = "Test Legal Name",
                VATNumber = "123456",
                RestaurantDescription = "A nice place",
                ContactEmail = "contact@test.com",
                ContactPhone = "123456789",
                Address = new AddressDto
                {
                    AddressLine1 = "Street 1",
                    AddressLine2 = "Suite 100",
                    City = "Test City",
                    ZipCode = "12345",
                    Country = "Test Country"
                },
                OperatingHours = new List<OperatingHoursDto>
                {
            new OperatingHoursDto { Day = DayEnum.Monday, OpeningHours = new TimeSpan(9, 0, 0), ClosingHours = new TimeSpan(17, 0, 0) }
        },
                FoodCategories = new List<FoodCategoryDto>
                {
            new FoodCategoryDto { Category = Category.Italian }
        }
            };

            _dataAccessMock.Setup(d => d.ExecuteStoredProcedure<int>("AddRestaurant", It.IsAny<object>()))
                           .Callback<string, object>((procName, dynamicParams) =>
                           {
                               if (dynamicParams is DynamicParameters parameters)
                               {
                                   parameters.Add("@RestaurantId", 1, DbType.Int32, ParameterDirection.Output);
                               }
                           })
                           .ReturnsAsync(1);

            var result = await _restaurantService.AddRestaurant(addRestaurantDto);

            Assert.Equal(1, result);
            _dataAccessMock.Verify(d => d.ExecuteStoredProcedure<int>("AddRestaurant", It.IsAny<object>()), Times.Once);
        }




        [Fact]
        public async Task AddMenuItem_ShouldReturn_NewMenuItemId()
        {
            var addMenuItemDto = new AddMenuItemDto
            {
                RestaurantId = 1,
                Name = "Test Dish",
                Description = "A tasty dish",
                Price = 9.99M,
                Image = new byte[0]
            };

            _dataAccessMock.Setup(d => d.InsertAndGetId<int?>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(5);

            var result = await _restaurantService.AddMenuItem(addMenuItemDto);

            Assert.Equal(5, result);
            _dataAccessMock.Verify(d => d.InsertAndGetId<int?>(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task GetRestaurantById_ShouldReturn_RestaurantDto()
        {
            var restaurantId = 1;
            var expectedRestaurant = new RestaurantDto
            {
                Id = restaurantId,
                RestaurantName = "Test Restaurant",
                Address = new AddressDto
                {
                    AddressLine1 = "Street 1",
                    City = "Test City",
                    ZipCode = "12345",
                    Country = "Test Country"
                },
                MenuItems = new List<MenuItemDto>(),
                OperatingHours = new List<OperatingHoursDto>(),
                FoodCategories = new List<FoodCategoryDto>()
            };

            _dataAccessMock.Setup(d => d.GetById<RestaurantDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(expectedRestaurant);
            _dataAccessMock.Setup(d => d.GetAll<MenuItemDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new List<MenuItemDto>());
            _dataAccessMock.Setup(d => d.GetAll<OperatingHoursDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new List<OperatingHoursDto>());
            _dataAccessMock.Setup(d => d.GetAll<FoodCategoryDto>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new List<FoodCategoryDto>());

            var result = await _restaurantService.GetRestaurantById(restaurantId);

            Assert.NotNull(result);
            Assert.Equal(restaurantId, result?.Id);
            Assert.Equal("Test Restaurant", result?.RestaurantName);
            _dataAccessMock.Verify(d => d.GetById<RestaurantDto>(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            _dataAccessMock.Verify(d => d.GetAll<MenuItemDto>(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task UpdateRestaurant_ShouldReturn_AffectedRows()
        {
            var updateRestaurantDto = new UpdateRestaurantDto
            {
                Id = 1,
                RestaurantName = "Updated Restaurant",
                LegalName = "Updated Legal Name",
                VATNumber = "654321",
                RestaurantDescription = "An even nicer place",
                ContactEmail = "updatedcontact@test.com",
                ContactPhone = "987654321",
            };

            _dataAccessMock.Setup(d => d.ExecuteStoredProcedure<int>("UpdateRestaurant", It.IsAny<object>()))
                           .ReturnsAsync(1);

            var result = await _restaurantService.UpdateRestaurant(updateRestaurantDto);

            Assert.Equal(1, result);
            _dataAccessMock.Verify(d => d.ExecuteStoredProcedure<int>("UpdateRestaurant", It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task DeleteRestaurant_ShouldReturn_AffectedRows()
        {
            int restaurantId = 1;

            _dataAccessMock.Setup(d => d.Delete(It.IsAny<string>(), It.IsAny<object>()))
                           .ReturnsAsync(1);

            var result = await _restaurantService.DeleteRestaurant(restaurantId);

            Assert.Equal(1, result);
            _dataAccessMock.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task RemoveMenuItem_ShouldReturn_AffectedRows()
        {

            int restaurantId = 1;
            int menuItemId = 1;

            _dataAccessMock.Setup(d => d.Delete(It.IsAny<string>(), It.IsAny<object>()))
                           .ReturnsAsync(1);

            var result = await _restaurantService.RemoveMenuItem(restaurantId, menuItemId);

            Assert.Equal(1, result);
            _dataAccessMock.Verify(d => d.Delete(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task GetAllRestaurants_ShouldReturn_ListOfRestaurants()
        {
            var restaurants = new List<RestaurantDto>
            {
        new RestaurantDto { Id = 1, RestaurantName = "Restaurant 1" },
        new RestaurantDto { Id = 2, RestaurantName = "Restaurant 2" }
    };

            _dataAccessMock.Setup(d => d.GetAll<RestaurantDto>(It.IsAny<string>(), null))
                           .ReturnsAsync(restaurants);

            var result = await _restaurantService.GetAllRestaurants();

            Assert.Equal(2, result.Count);
            _dataAccessMock.Verify(d => d.GetAll<RestaurantDto>(It.IsAny<string>(), null), Times.Once);
        }

    }
}
