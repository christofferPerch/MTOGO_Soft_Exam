using Microsoft.AspNetCore.Mvc;
using MTOGO.Services.AuthAPI.Models.Dto;
using MTOGO.Services.AuthAPI.Services.IServices;
using System.Security.Claims;

namespace MTOGO.Services.AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        private readonly IAuthService _authService;
        protected ResponseDto _response;
        public AuthAPIController(IAuthService authService)
        {
            _authService = authService;
            _response = new();
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
        {

            var errorMessage = await _authService.Register(model);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                _response.IsSuccess = false;
                _response.Message = errorMessage;
                return BadRequest(_response);
            }
            return Ok(_response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            var loginResponse = await _authService.Login(model);
            if (loginResponse.User == null)
            {
                _response.IsSuccess = false;
                _response.Message = "Username or password is incorrect";
                return BadRequest(_response);
            }
            _response.Result = loginResponse;
            return Ok(_response);

        }

        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto model)
        {
            var assignRoleSuccessful = await _authService.AssignRole(model.Email, model.Role.ToUpper());
            if (!assignRoleSuccessful)
            {
                _response.IsSuccess = false;
                _response.Message = "Error encountered";
                return BadRequest(_response);
            }
            return Ok(_response);

        }

        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromQuery] string userId, [FromBody] UpdateProfileDto updateProfileDto)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "User ID is required." });
            }

            var updatedUser = await _authService.UpdateUserSettings(userId, updateProfileDto);

            if (updatedUser != null)
            {
                return Ok(new { isSuccess = true, result = updatedUser, message = "Profile updated successfully." });
            }

            return BadRequest(new { isSuccess = false, message = "Failed to update profile." });
        }

        [HttpDelete("DeleteProfile")]
        public async Task<IActionResult> DeleteProfile([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "User ID not found." });
            }

            var isDeleted = await _authService.DeleteAccount(userId);

            if (isDeleted)
            {
                return Ok(new { message = "Account deleted successfully." });
            }

            return BadRequest(new { message = "Failed to delete account." });
        }
    }
}
