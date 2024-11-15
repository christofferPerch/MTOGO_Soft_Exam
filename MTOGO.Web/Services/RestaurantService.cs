﻿using MTOGO.Web.Models;
using MTOGO.Web.Services.IServices;
using MTOGO.Web.Utility;

namespace MTOGO.Web.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IBaseService _baseService;

        public RestaurantService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> GetAllRestaurantsAsync()
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = $"{SD.RestaurantAPIBase}/api/restaurant/allRestaurants"
            });
        }
    }
}
