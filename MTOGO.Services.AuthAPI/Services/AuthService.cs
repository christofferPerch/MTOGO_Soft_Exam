using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MTOGO.Services.AuthAPI.Data;
using MTOGO.Services.AuthAPI.Models;
using MTOGO.Services.AuthAPI.Models.Dto;
using MTOGO.Services.AuthAPI.Services.IServices;

namespace MTOGO.Services.AuthAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ILogger<AuthService> _logger;


        public AuthService(AppDbContext db, IJwtTokenGenerator jwtTokenGenerator,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            ILogger<AuthService> logger)
        {
            _db = db;
            _jwtTokenGenerator = jwtTokenGenerator;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<bool> AssignRole(string email, string roleName)
        {
            var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user != null)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
                await _userManager.AddToRoleAsync(user, roleName);
                return true;
            }
            return false;

        }

        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName.ToLower() == loginRequestDto.UserName.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

            if (user == null || isValid == false)
            {
                return new LoginResponseDto() { User = null, Token = "" };
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenGenerator.GenerateToken(user, roles);

            UserDto userDTO = new()
            {
                Email = user.Email,
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                ZipCode = user.ZipCode,
                Country = user.Country
            };

            LoginResponseDto loginResponseDto = new LoginResponseDto()
            {
                User = userDTO,
                Token = token
            };

            return loginResponseDto;
        }

        public async Task<string> Register(RegistrationRequestDto registrationRequestDto)
        {
            ApplicationUser user = new()
            {
                UserName = registrationRequestDto.Email,
                Email = registrationRequestDto.Email,
                NormalizedEmail = registrationRequestDto.Email.ToUpper(),
                FirstName = registrationRequestDto.FirstName,
                PhoneNumber = registrationRequestDto.PhoneNumber,
                LastName = registrationRequestDto.LastName,
                Address = registrationRequestDto.Address,
                City = registrationRequestDto.City,
                ZipCode = registrationRequestDto.ZipCode,
                Country = registrationRequestDto.Country,
            };

            try
            {
                var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("User"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("User"));
                    }

                    await _userManager.AddToRoleAsync(user, "User");

                    UserDto userDto = new()
                    {
                        Email = user.Email,
                        UserId = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Address = user.Address,
                        City = user.City,
                        ZipCode = user.ZipCode,
                        Country = user.Country,
                        PhoneNumber = user.PhoneNumber
                    };

                    return "";
                }
                else
                {
                    return result.Errors.FirstOrDefault()?.Description ?? "Registration failed.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return "Error encountered during registration.";
            }
        }

        public async Task<UserDto?> UpdateUserSettings(string userId, UpdateProfileDto updateProfileDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found.");
                return null;
            }

            bool isUpdated = false;

            if (!string.IsNullOrEmpty(updateProfileDto.Email) && updateProfileDto.Email != user.Email)
            {
                user.Email = updateProfileDto.Email;
                user.UserName = updateProfileDto.Email;
                isUpdated = true;
            }

            if (!string.IsNullOrEmpty(updateProfileDto.PhoneNumber) && updateProfileDto.PhoneNumber != user.PhoneNumber)
            {
                user.PhoneNumber = updateProfileDto.PhoneNumber;
                isUpdated = true;
            }

            if (!string.IsNullOrEmpty(updateProfileDto.Address) && updateProfileDto.Address != user.Address)
            {
                user.Address = updateProfileDto.Address;
                isUpdated = true;
            }
            if (!string.IsNullOrEmpty(updateProfileDto.City) && updateProfileDto.City != user.City)
            {
                user.City = updateProfileDto.City;
                isUpdated = true;
            }
            if (!string.IsNullOrEmpty(updateProfileDto.ZipCode) && updateProfileDto.ZipCode != user.ZipCode)
            {
                user.ZipCode = updateProfileDto.ZipCode;
                isUpdated = true;
            }
            if (!string.IsNullOrEmpty(updateProfileDto.Country) && updateProfileDto.Country != user.Country)
            {
                user.Country = updateProfileDto.Country;
                isUpdated = true;
            }

            if (isUpdated)
            {
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to update user profile.");
                    return null;
                }
            }

            return new UserDto
            {
                UserId = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                ZipCode = user.ZipCode,
                Country = user.Country,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        public async Task<bool> DeleteAccount(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found for deletion.");
                return false;
            }

            try
            {
                var result = await _userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User with ID {userId} has been successfully deleted.");
                    return true;
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        _logger.LogError($"Error deleting user with ID {userId}: {error.Description}");
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception occurred while deleting user with ID {userId}");
                return false;
            }
        }
    }
}
