﻿using MTOGO.Services.AuthAPI.Models.Dto;

namespace MTOGO.Services.AuthAPI.Services.IServices
{
    public interface IAuthService
    {
        Task<string> Register(RegistrationRequestDto registrationRequestDto);
        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
        Task<bool> AssignRole(string email, string roleName);
        Task<UserDto?> UpdateUserSettings(string userId, UpdateProfileDto updateProfileDto);
        Task<bool> DeleteAccount(string userId);
    }
}
