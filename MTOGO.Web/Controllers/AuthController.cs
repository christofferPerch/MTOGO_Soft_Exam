﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MTOGO.Web.Models.Auth;
using MTOGO.Web.Models.Order;
using MTOGO.Web.Models.Restaurant;
using MTOGO.Web.Services.IServices;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MTOGO.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        private readonly IOrderService _orderService;
        private readonly IRestaurantService _restaurantService;
        public AuthController(IAuthService authService, ITokenProvider tokenProvider, IOrderService orderService, IRestaurantService restaurantService)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
            _orderService = orderService;
            _restaurantService = restaurantService;
        }

        [HttpGet]
        public IActionResult Profile()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            var userId = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User ID not found in session." });
            }

            var response = await _authService.UpdateProfileSettings(userId, updateProfileDto);

            if (response != null && response.IsSuccess)
            {
                var updatedUser = JsonConvert.DeserializeObject<UserDto>(Convert.ToString(response.Result));
                if (updatedUser != null)
                {
                    await UpdateUserSession(updatedUser);
                    var newToken = await GenerateUpdatedToken(updatedUser);
                    _tokenProvider.SetToken(newToken);

                    return Json(new { success = true, token = newToken });
                }
            }

            return Json(new { success = false, message = response?.Message ?? "Failed to update profile." });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId == null)
            {
                TempData["error"] = "Unable to identify the user for deletion.";
                return BadRequest(new { message = "User ID not found" });
            }

            var response = await _authService.DeleteProfile(userId);

            if (response != null && response.IsSuccess)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["success"] = "Your account has been successfully deleted.";
                return Ok(new { message = "Account deleted successfully" });
            }

            TempData["error"] = response?.Message ?? "Failed to delete account.";
            return BadRequest(new { message = "Failed to delete account" });
        }
        private async Task<string> GenerateUpdatedToken(UserDto userDto)
        {
            var loginResponse = await _authService.LoginAsync(new LoginRequestDto
            {
                UserName = userDto.Email
            });

            var loginResponseDto = JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(loginResponse?.Result));
            return loginResponseDto?.Token ?? string.Empty;
        }

        private async Task UpdateUserSession(UserDto user)
        {
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            identity.AddClaim(new Claim("UserId", user.UserId ?? string.Empty));
            identity.AddClaim(new Claim("Email", user.Email ?? string.Empty));
            identity.AddClaim(new Claim("FirstName", user.FirstName ?? string.Empty));
            identity.AddClaim(new Claim("LastName", user.LastName ?? string.Empty));
            identity.AddClaim(new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()));
            identity.AddClaim(new Claim("ZipCode", user.ZipCode ?? string.Empty));
            identity.AddClaim(new Claim("Address", user.Address ?? string.Empty));
            identity.AddClaim(new Claim("Country", user.Country ?? string.Empty));
            identity.AddClaim(new Claim("PhoneNumber", user.PhoneNumber ?? string.Empty));
            identity.AddClaim(new Claim("City", user.City ?? string.Empty));

            var existingClaims = User.Claims.ToList();
            var roleClaim = existingClaims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (!string.IsNullOrEmpty(roleClaim))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim));
            }

            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginRequestDto());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto loginRequest)
        {
            if (!ModelState.IsValid) return View(loginRequest);

            var responseDto = await _authService.LoginAsync(loginRequest);
            if (responseDto != null && responseDto.IsSuccess)
            {
                var loginResponse = JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(responseDto.Result));
                if (loginResponse != null)
                {
                    await SignInUser(loginResponse);
                    _tokenProvider.SetToken(loginResponse.Token);
                    return RedirectToAction("Index", "Home");
                }
            }

            TempData["error"] = responseDto?.Message ?? "Invalid login attempt.";
            return View(loginRequest);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegistrationRequestDto());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequestDto registrationRequest)
        {
            if (!ModelState.IsValid) return View(registrationRequest);

            var registerResponse = await _authService.RegisterAsync(registrationRequest);
            if (registerResponse != null && registerResponse.IsSuccess)
            {
                if (string.IsNullOrEmpty(registrationRequest.Role))
                {
                    registrationRequest.Role = "Customer";
                }

                var assignRoleResponse = await _authService.AssignRoleAsync(registrationRequest);
                if (assignRoleResponse != null && assignRoleResponse.IsSuccess)
                {
                    TempData["success"] = "Registration successful. Please log in.";
                    return RedirectToAction("Login");
                }
            }

            TempData["error"] = registerResponse?.Message ?? "Registration failed.";
            return View(registrationRequest);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _tokenProvider.ClearToken();
            return RedirectToAction("Index", "Home");
        }

        private async Task SignInUser(LoginResponseDto model)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(model.Token);

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            identity.AddClaim(new Claim("UserId", model.User.UserId ?? string.Empty));
            identity.AddClaim(new Claim("Email", model.User.Email ?? string.Empty));
            identity.AddClaim(new Claim("FirstName", model.User.FirstName ?? string.Empty));
            identity.AddClaim(new Claim("LastName", model.User.LastName ?? string.Empty));
            identity.AddClaim(new Claim(ClaimTypes.Name, $"{model.User.FirstName} {model.User.LastName}".Trim()));
            identity.AddClaim(new Claim("ZipCode", model.User.ZipCode ?? string.Empty));
            identity.AddClaim(new Claim("Address", model.User.Address ?? string.Empty));
            identity.AddClaim(new Claim("Country", model.User.Country ?? string.Empty));
            identity.AddClaim(new Claim("PhoneNumber", model.User.PhoneNumber ?? string.Empty));
            identity.AddClaim(new Claim("City", model.User.City ?? string.Empty));

            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            if (!string.IsNullOrEmpty(roleClaim))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim));
            }

            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        [HttpGet]
        public async Task<IActionResult> GetActiveOrders()
        {
            var userId = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User ID is required." });
            }

            var response = await _orderService.GetActiveOrders(userId);
            if (response != null && response.IsSuccess && response.Result != null)
            {
                try
                {
                    var activeOrders = JsonConvert.DeserializeObject<List<OrderManagementDto>>(response.Result.ToString());

                    // Fetch menu item names
                    foreach (var order in activeOrders)
                    {
                        foreach (var item in order.Items)
                        {
                            var cartDetailsResponse = await _restaurantService.GetCartDetailsAsync(item.RestaurantId, item.MenuItemId);
                            if (cartDetailsResponse?.IsSuccess == true && cartDetailsResponse.Result != null)
                            {
                                var cartDetails = JsonConvert.DeserializeObject<CartDetailsDto>(cartDetailsResponse.Result.ToString());
                                item.MenuItemName = cartDetails?.Name ?? "Unknown Item";
                            }
                            else
                            {
                                item.MenuItemName = "Unknown Item";
                            }
                        }
                    }

                    return Json(new { success = true, data = activeOrders });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Failed to process active orders." });
                }
            }

            return Json(new { success = false, message = response?.Message ?? "Failed to load active orders." });
        }



        [HttpGet]
        public async Task<IActionResult> GetOrderHistory()
        {
            var userId = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User ID is required." });
            }

            var response = await _orderService.GetOrderHistory(userId);
            if (response != null && response.IsSuccess && response.Result != null)
            {
                try
                {
                    var orderHistory = JsonConvert.DeserializeObject<List<OrderManagementDto>>(response.Result.ToString());

                    // Fetch menu item names
                    foreach (var order in orderHistory)
                    {
                        foreach (var item in order.Items)
                        {
                            var cartDetailsResponse = await _restaurantService.GetCartDetailsAsync(item.RestaurantId, item.MenuItemId);
                            if (cartDetailsResponse?.IsSuccess == true && cartDetailsResponse.Result != null)
                            {
                                var cartDetails = JsonConvert.DeserializeObject<CartDetailsDto>(cartDetailsResponse.Result.ToString());
                                item.MenuItemName = cartDetails?.Name ?? "Unknown Item";
                            }
                            else
                            {
                                item.MenuItemName = "Unknown Item";
                            }
                        }
                    }

                    return Json(new { success = true, data = orderHistory });
                }
                catch (Exception ex)
                {
                    // Log the exception if needed
                    return Json(new { success = false, message = "Failed to process order history." });
                }
            }

            return Json(new { success = false, message = response?.Message ?? "Failed to load order history." });
        }


    }
}
