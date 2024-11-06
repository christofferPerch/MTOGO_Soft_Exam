using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MTOGO.Services.AuthAPI.Data;
using MTOGO.Services.AuthAPI.Models;
using MTOGO.Services.AuthAPI.Models.Dto;
using MTOGO.Services.AuthAPI.Services;

namespace MTOGO.UnitTests.Auth
{
    public class AuthUnitTests
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AppDbContext _dbContext;
        private readonly AuthService _authService;

        public AuthUnitTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "AuthServiceTestDb")
                .Options;
            _dbContext = new AppDbContext(_dbContextOptions);

            _userManagerMock = MockUserManager();
            _roleManagerMock = MockRoleManager();
            _loggerMock = new Mock<ILogger<AuthService>>();

            var jwtOptions = Options.Create(new JwtOptions
            {
                Secret = "THISISASUPERSECRETKEYTHISISASUPERSECRETKEY123THISISASUPERSECRETKEY",
                Issuer = "mtogo-auth-api",
                Audience = "mtogo-client"
            });
            _jwtTokenGenerator = new JwtTokenGenerator(jwtOptions);

            _authService = new AuthService(
                _dbContext,
                _jwtTokenGenerator,
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

        [Fact]
        public async Task AssignRole_UserExistsAndRoleCreated_ReturnsTrue()
        {
            string email = "testuser@example.com";
            string roleName = "Admin";
            var user = new ApplicationUser
            {
                Email = email,
                UserName = "testuser",
                FirstName = "Test",
                LastName = "User",
                Address = "123 Main St",
                City = "Test City",
                ZipCode = "12345",
                Country = "Test Country"
            };
            _dbContext.ApplicationUsers.Add(user);
            await _dbContext.SaveChangesAsync();

            _roleManagerMock.Setup(r => r.RoleExistsAsync(roleName)).ReturnsAsync(false);
            _roleManagerMock.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(user, roleName)).ReturnsAsync(IdentityResult.Success);

            var result = await _authService.AssignRole(email, roleName);

            Assert.True(result);
            _roleManagerMock.Verify(r => r.CreateAsync(It.IsAny<IdentityRole>()), Times.Once);
            _userManagerMock.Verify(u => u.AddToRoleAsync(user, roleName), Times.Once);
        }

        [Fact]
        public async Task AssignRole_UserDoesNotExist_ReturnsFalse()
        {
            string email = "nonexistent@example.com";
            string roleName = "Admin";

            var result = await _authService.AssignRole(email, roleName);

            Assert.False(result);
        }

        [Fact]
        public async Task Login_ValidUser_ReturnsLoginResponseWithToken()
        {
            var loginRequest = new LoginRequestDto { UserName = "testuser", Password = "password" };
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "testuser@example.com",
                FirstName = "Test",
                LastName = "User",
                Address = "123 Main St",
                City = "Test City",
                ZipCode = "12345",
                Country = "Test Country"
            };
            _dbContext.ApplicationUsers.Add(user);
            await _dbContext.SaveChangesAsync();

            _userManagerMock.Setup(u => u.CheckPasswordAsync(It.IsAny<ApplicationUser>(), loginRequest.Password))
                            .ReturnsAsync(true);

            _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>()))
                            .ReturnsAsync(new List<string> { "User" });

            var result = await _authService.Login(loginRequest);

            Assert.NotNull(result);
            Assert.NotNull(result.User);  
            Assert.False(string.IsNullOrEmpty(result.Token));  
        }


        [Fact]
        public async Task Login_InvalidUser_ReturnsEmptyLoginResponse()
        {
            var loginRequest = new LoginRequestDto { UserName = "invaliduser", Password = "password" };

            var result = await _authService.Login(loginRequest);

            Assert.NotNull(result);
            Assert.Null(result.User);
            Assert.Equal(string.Empty, result.Token);
        }

        [Fact]
        public async Task Register_ValidRequest_ReturnsEmptyMessage()
        {
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

            var expectedUser = new ApplicationUser
            {
                UserName = registrationRequest.Email,
                Email = registrationRequest.Email,
                FirstName = registrationRequest.FirstName,
                LastName = registrationRequest.LastName,
                Address = registrationRequest.Address,
                City = registrationRequest.City,
                ZipCode = registrationRequest.ZipCode,
                Country = registrationRequest.Country,
                PhoneNumber = registrationRequest.PhoneNumber,
            };

            _userManagerMock.Setup(u => u.CreateAsync(
                It.Is<ApplicationUser>(user =>
                    user.Email == expectedUser.Email &&
                    user.UserName == expectedUser.UserName &&
                    user.FirstName == expectedUser.FirstName &&
                    user.LastName == expectedUser.LastName &&
                    user.Address == expectedUser.Address &&
                    user.City == expectedUser.City &&
                    user.ZipCode == expectedUser.ZipCode &&
                    user.Country == expectedUser.Country &&
                    user.PhoneNumber == expectedUser.PhoneNumber
                ),
                registrationRequest.Password
            )).ReturnsAsync(IdentityResult.Success);

            _roleManagerMock.Setup(r => r.RoleExistsAsync("User"))
                            .ReturnsAsync(true);

            _userManagerMock.Setup(u => u.AddToRoleAsync(It.Is<ApplicationUser>(user => user.Email == expectedUser.Email), "User"))
                            .ReturnsAsync(IdentityResult.Success);

            var result = await _authService.Register(registrationRequest);

            Assert.Equal(string.Empty, result); 
        }

        [Fact]
        public async Task Register_InvalidRequest_ReturnsErrorMessage()
        {
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

            var result = await _authService.Register(registrationRequest);

            Assert.Equal("Registration failed.", result);
        }
    }
}