﻿using MTOGO.Web.Models;
using MTOGO.Web.Models.Auth;

namespace MTOGO.Web.Services.IServices
{
    public interface IAuthService
    {
        Task<ResponseDto?> LoginAsync(LoginRequestDto loginRequestDto);
        Task<ResponseDto?> RegisterAsync(RegistrationRequestDto registrationRequestDto);
        Task<ResponseDto?> AssignRoleAsync(RegistrationRequestDto registrationRequestDto);
        Task<ResponseDto?> UpdateProfileSettings(string userId, UpdateProfileDto updateProfileDto);
        Task<ResponseDto?> DeleteProfile(string userId);
    }
}
