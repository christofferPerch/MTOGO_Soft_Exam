using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MTOGO.Services.AuthAPI.Models;
using MTOGO.Services.AuthAPI.Models.Dto;
using MTOGO.Services.AuthAPI.Services;
using MTOGO.Services.AuthAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using MTOGO.Services.AuthAPI.Services.IServices;
using System.Linq.Expressions;

namespace MTOGO.UnitTests.Auth
{
    public class AuthUnitTest
    {
        private readonly Mock<AppDbContext> _dbContextMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _authService;

        public AuthUnitTest()
        {
            _dbContextMock = new Mock<AppDbContext>();
            _userManagerMock = MockUserManager();
            _roleManagerMock = MockRoleManager();
            _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            _authService = new AuthService(
                _dbContextMock.Object,
                _jwtTokenGeneratorMock.Object,
                _userManagerMock.Object,
                _roleManagerMock.Object,
                _loggerMock.Object
            );
        }

        private Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            return new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object, null, null, null, null, null, null, null, null);
        }

        private Mock<RoleManager<IdentityRole>> MockRoleManager()
        {
            return new Mock<RoleManager<IdentityRole>>(
                new Mock<IRoleStore<IdentityRole>>().Object, null, null, null, null);
        }

        // Test AssignRole Method
        [Fact]
        public async Task AssignRole_UserExistsAndRoleCreated_ReturnsTrue()
        {
            // Arrange
            string email = "testuser@example.com";
            string roleName = "Admin";
            var user = new ApplicationUser { Email = email };
            _dbContextMock.Setup(db => db.ApplicationUsers.FirstOrDefaultAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(user);
            _roleManagerMock.Setup(r => r.RoleExistsAsync(roleName)).ReturnsAsync(false);
            _roleManagerMock.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(user, roleName)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.AssignRole(email, roleName);

            // Assert
            Assert.True(result);
            _roleManagerMock.Verify(r => r.CreateAsync(It.IsAny<IdentityRole>()), Times.Once);
            _userManagerMock.Verify(u => u.AddToRoleAsync(user, roleName), Times.Once);
        }

        [Fact]
        public async Task AssignRole_UserDoesNotExist_ReturnsFalse()
        {
            // Arrange
            string email = "nonexistent@example.com";
            string roleName = "Admin";
            _dbContextMock.Setup(db => db.ApplicationUsers.FirstOrDefaultAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _authService.AssignRole(email, roleName);

            // Assert
            Assert.False(result);
        }

        // Test Login Method
        [Fact]
        public async Task Login_ValidUser_ReturnsLoginResponseWithToken()
        {
            // Arrange
            var loginRequest = new LoginRequestDto { UserName = "testuser", Password = "password" };
            var user = new ApplicationUser { UserName = "testuser", Email = "testuser@example.com" };
            _dbContextMock.Setup(db => db.ApplicationUsers.FirstOrDefaultAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(user);
            _userManagerMock.Setup(u => u.CheckPasswordAsync(user, loginRequest.Password)).ReturnsAsync(true);
            _jwtTokenGeneratorMock.Setup(j => j.GenerateToken(user, It.IsAny<IList<string>>())).Returns("generated_jwt_token");

            // Act
            var result = await _authService.Login(loginRequest);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.User);
            Assert.Equal("generated_jwt_token", result.Token);
        }

        [Fact]
        public async Task Login_InvalidUser_ReturnsEmptyLoginResponse()
        {
            // Arrange
            var loginRequest = new LoginRequestDto { UserName = "invaliduser", Password = "password" };
            _dbContextMock.Setup(db => db.ApplicationUsers.FirstOrDefaultAsync(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((ApplicationUser)null);

            // Act
            var result = await _authService.Login(loginRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.User);
            Assert.Equal(string.Empty, result.Token);
        }

        // Test Register Method
        [Fact]
        public async Task Register_ValidRequest_ReturnsEmptyMessage()
        {
            // Arrange
            var registrationRequest = new RegistrationRequestDto
            {
                Email = "newuser@example.com",
                Password = "password",
                FirstName = "First",
                LastName = "Last",
                Address = "123 Main St",
                City = "Cityville",
                ZipCode = "12345",
                Country = "Countryland",
                PhoneNumber = "1234567890"
            };
            var user = new ApplicationUser { Email = registrationRequest.Email };
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), registrationRequest.Password))
                            .ReturnsAsync(IdentityResult.Success);
            _roleManagerMock.Setup(r => r.RoleExistsAsync("User")).ReturnsAsync(true);
            _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User")).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _authService.Register(registrationRequest);

            // Assert
            Assert.Equal(string.Empty, result); // Assuming empty string indicates success
        }

        [Fact]
        public async Task Register_InvalidRequest_ReturnsErrorMessage()
        {
            // Arrange
            var registrationRequest = new RegistrationRequestDto
            {
                Email = "invaliduser@example.com",
                Password = "password",
                FirstName = "First",
                LastName = "Last"
            };
            var identityError = new IdentityError { Description = "Registration failed." };
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), registrationRequest.Password))
                            .ReturnsAsync(IdentityResult.Failed(identityError));

            // Act
            var result = await _authService.Register(registrationRequest);

            // Assert
            Assert.Equal("Registration failed.", result);
        }
    }
}