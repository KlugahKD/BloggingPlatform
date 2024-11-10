using System.Linq.Expressions;
using BloggingPlatform.Business.Models.Requests;
using BloggingPlatform.Business.Services.Providers;
using BloggingPlatform.Data.Entities;
using BloggingPlatform.Data.Repositories.Interface;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using BloggingPlatform.Business.Models.BaseModels;
using BloggingPlatform.Business.Services.Interfaces;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace BloggingPlatForm.UnitTest
{
    public class UserServiceTests
    {
        private readonly UserService _sut;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private IConfiguration _configuration;

        public UserServiceTests()
        {
            ILogger<UserService> logger = Substitute.For<ILogger<UserService>>();
            _userRepository = Substitute.For<IGenericRepository<User>>();
            _passwordHasher = Substitute.For<IPasswordHasher>();
            _configuration = Substitute.For<IConfiguration>();
            

            _sut = new UserService(
                _userRepository,
                logger,
                _passwordHasher,
                _configuration
            );

        }

        #region RegisterUserAsync


        [Fact]
        public async Task RegisterUserAsync_Should_Return_BadRequest_When_Phone_Number_Is_Not_Valid()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, "TestUser") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var userPrincipal = new ClaimsPrincipal(identity);
            var userRequest = new AddUserRequest
                { PhoneNumber = "invalid", Password = "password123", FullName = "Dan K" };

            // Act
            var result = await _sut.RegisterUserAsync(userRequest, userPrincipal);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(400);
                result.Message.Should().Be("Invalid phone number");
            }
        }

        [Fact]
        public async Task RegisterUserAsync_Should_Return_BadRequest_When_Password_Is_Too_Short()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, "TestUser") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var userPrincipal = new ClaimsPrincipal(identity);
            var userRequest = new AddUserRequest { PhoneNumber = "0274810934", Password = "123", FullName = "Dan K" };

            // Act
            var result = await _sut.RegisterUserAsync(userRequest, userPrincipal);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(400);
                result.Message.Should().Be("Password must be at least 6 characters long");
            }
        }

        [Fact]
        public async Task RegisterUserAsync_Should_Return_BadRequest_When_User_Already_Exists()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, "TestUser") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var userPrincipal = new ClaimsPrincipal(identity);
            var userRequest = new AddUserRequest
            {
                PhoneNumber = "0274810934", Email = "test@example.com", Password = "password123", FullName = "Dan K"
            };
            _userRepository.ExistsAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(true);

            // Act
            var result = await _sut.RegisterUserAsync(userRequest, userPrincipal);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(400);
                result.Message.Should().Be("User already exists");
            }
        }

        [Fact]
        public async Task RegisterUserAsync_Should_Return_FailedDependency_When_Saving_User_Fails()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, "TestUser") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var userPrincipal = new ClaimsPrincipal(identity);
            var userRequest = new AddUserRequest
            {
                PhoneNumber = "0274810934", Email = "test@example.com", Password = "password123", FullName = "Dan K"
            };
            _userRepository.ExistsAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(false);
            _userRepository.AddAsync(Arg.Any<User>()).Returns(false);
            _passwordHasher.HashPassword(Arg.Any<string>()).Returns("hashedPassword");

            // Act
            var result = await _sut.RegisterUserAsync(userRequest, userPrincipal);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(424);
                result.Message.Should().Be("Error occurred while saving user");
            }
        }

        [Fact]
        public async Task RegisterUserAsync_Should_Return_Ok_When_User_Is_Successfully_Registered()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, "TestUser") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var userPrincipal = new ClaimsPrincipal(identity);
            var userRequest = new AddUserRequest
            {
                PhoneNumber = "0274810934", Email = "test@example.com", Password = "password123", FullName = "Dan K"
            };
            _userRepository.ExistsAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(false);
            _userRepository.AddAsync(Arg.Any<User>()).Returns(true);
            _passwordHasher.HashPassword(Arg.Any<string>()).Returns("hashedPassword");

            // Act
            var result = await _sut.RegisterUserAsync(userRequest, userPrincipal);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(200);
            }
        }

        [Fact]
        public async Task RegisterUserAsync_Should_Return_FailedDependency_When_Exception_Occurs_While_Saving_User()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, "TestUser") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var userPrincipal = new ClaimsPrincipal(identity);
            var userRequest = new AddUserRequest
            {
                PhoneNumber = "0274810934", Email = "test@example.com", Password = "password123", FullName = "Dan K"
            };
            _userRepository.ExistsAsync(Arg.Any<Expression<Func<User, bool>>>())
                .Returns(Task.FromException<bool>(new Exception("Exception occurred")));
            _userRepository.AddAsync(Arg.Any<User>()).Returns(true);
            _passwordHasher.HashPassword(Arg.Any<string>()).Returns("hashedPassword");

            // Act
            var result = await _sut.RegisterUserAsync(userRequest, userPrincipal);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(500);
                result.Message.Should().Be("Something Bad Happened. Please try again later.");
            }
        }

        #endregion

        #region LoginAsync

        [Fact]
        public async Task LoginAsync_Should_Return_BadRequest_When_Phone_Number_Is_Not_Valid()
        {
            // Arrange
            var loginRequest = new LoginRequest { PhoneNumber = "invalid", Password = "password123" };

            // Act
            var result = await _sut.LoginAsync(loginRequest);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(400);
                result.Message.Should().Be("Invalid phone number");
            }
        }

        [Fact]
        public async Task LoginAsync_Should_Return_BadRequest_When_User_Does_Not_Exist()
        {
            // Arrange
            var loginRequest = new LoginRequest { PhoneNumber = "0274810934", Password = "password123" };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(null as User);

            // Act
            var result = await _sut.LoginAsync(loginRequest);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(400);
                result.Message.Should().Be("Incorrect Login Credentials");
            }
        }

        [Fact]
        public async Task LoginAsync_Should_Return_BadRequest_When_Password_Is_Incorrect()
        {
            // Arrange
            var loginRequest = new LoginRequest { PhoneNumber = "0274810934", Password = "wrongpassword" };
            var user = new User {Id = "1", PhoneNumber = "0274810934", Password = "hashedPassword", IsActive = true, FullName = "Dan K"};
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
            _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

            // Act
            var result = await _sut.LoginAsync(loginRequest);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(400);
                result.Message.Should().Be("Incorrect Login Credentials");
            }
        }

        // [Fact]
        // public async Task LoginAsync_Should_Return_Ok_When_Login_Is_Successful()
        // {
        //     // Arrange
        //     var loginRequest = new LoginRequest { PhoneNumber = "0274810934", Password = "password123" };
        //     var user = new User
        //     {
        //         Id = "1", FullName = "Test User", Email = "test@example.com", PhoneNumber = "0274810934",
        //         Password = "hashedPassword", Role = "User", IsActive = true
        //     };
        //     _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
        //     _passwordHasher.VerifyPassword(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        //     
        //    
        //     _configuration = new ConfigurationBuilder()
        //         .SetBasePath(Directory.GetCurrentDirectory())
        //         .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
        //         .Build();
        //
        //     // Act
        //     var result = await _sut.LoginAsync(loginRequest);
        //
        //     // Assert
        //     using (new AssertionScope())
        //     {
        //         result.Should().NotBeNull();
        //         result.Code.Should().Be(200);
        //         result.Data.Should().NotBeNullOrEmpty();
        //     }
        // }
        
        [Fact]
        public async Task LoginAsync_Should_Return_InternalServerError_When_Exception_Occurs()
        {
            // Arrange
            var loginRequest = new LoginRequest { PhoneNumber = "0274810934", Password = "password123" };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>())!
                .Returns(Task.FromException<User>(new Exception("Exception occurred")));
            
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .Build();

            // Act
            var result = await _sut.LoginAsync(loginRequest);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(500);
                result.Message.Should().Be("Something Bad Happened. Please try again later.");
            }
        }
        #endregion
        
        #region GetUserByIdAsync
        
        [Fact]
        public async Task GetUserByIdAsync_Should_Return_Ok_When_User_Is_Found()
        {
            // Arrange
            var userId = "1";
            var user = new User { Id = userId, IsActive = true, FullName = "Test User", PhoneNumber = "0274810934" , Password = "hashedPassword"};
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);

            // Act
            var result = await _sut.GetUserByIdAsync(userId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(200);
                result.Data.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task GetUserByIdAsync_Should_Return_NotFound_When_User_Is_Not_Found()
        {
            // Arrange
            var userId = "1";
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(null as User);

            // Act
            var result = await _sut.GetUserByIdAsync(userId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(404);
                result.Message.Should().Be("User does not exist");
            }
        }

        [Fact]
        public async Task GetUserByIdAsync_Should_Return_InternalServerError_When_Exception_Occurs()
        {
            // Arrange
            var userId = "1";
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>())!
                .Returns(Task.FromException<User>(new Exception("Exception occurred")));

            // Act
            var result = await _sut.GetUserByIdAsync(userId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(500);
                result.Message.Should().Be("Something Bad Happened. Please try again later.");
            }
        }
        #endregion
        
        #region UpdateUserAsync
        
        [Fact]
        public async Task UpdateUserAsync_Should_Return_Ok_When_User_Is_Updated_Successfully()
        {
            // Arrange
            var userId = "1";
            var userRequest = new UpdateUserRequest { FullName = "Updated Name", Email = "updated@example.com" };
            var user = new User { Id = userId, IsActive = true, FullName = "Original Name", Email = "original@example.com", Password = "hashedPassword", PhoneNumber = "0274810934" };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
            _userRepository.UpdateAsync(user).Returns(true);

            // Act
            var result = await _sut.UpdateUserAsync(userId, userRequest);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(200);
                result.Data.Should().NotBeNull();
                result.Data.FullName.Should().Be("Updated Name");
                result.Data.Email.Should().Be("updated@example.com");
            }
        }

        [Fact]
        public async Task UpdateUserAsync_Should_Return_NotFound_When_User_Is_Not_Found()
        {
            // Arrange
            var userId = "1";
            var userRequest = new UpdateUserRequest { FullName = "Updated Name", Email = "updated@example.com" };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns((User)null);

            // Act
            var result = await _sut.UpdateUserAsync(userId, userRequest);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(404);
                result.Message.Should().Be("User not found");
            }
        }

        [Fact]
        public async Task UpdateUserAsync_Should_Return_FailedDependency_When_Update_Fails()
        {
            // Arrange
            var userId = "1";
            var userRequest = new UpdateUserRequest { FullName = "Updated Name", Email = "updated@example.com" };
            var user = new User { Id = userId, IsActive = true, FullName = "Original Name", Email = "original@example.com", PhoneNumber = "0274810934", Password = "hashedPassword" };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
            _userRepository.UpdateAsync(user).Returns(false);

            // Act
            var result = await _sut.UpdateUserAsync(userId, userRequest);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(424);
                result.Message.Should().Be("Error occurred while updating user");
            }
        }

        [Fact]
        public async Task UpdateUserAsync_Should_Return_InternalServerError_When_Exception_Occurs()
        {
            // Arrange
            var userId = "1";
            var userRequest = new UpdateUserRequest { FullName = "Updated Name", Email = "updated@example.com", PhoneNumber = "0274810934", Role = "Admin"};
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>())
                .Returns(Task.FromException<User>(new Exception("Exception occurred")));

            // Act
            var result = await _sut.UpdateUserAsync(userId, userRequest);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(500);
                result.Message.Should().Be("Something Bad Happened. Please try again later.");
            }
        }
        #endregion
        
        
        #region DeleteUserAsync
         [Fact]
        public async Task DeleteUserAsync_Should_Return_Ok_When_User_Is_Deleted_Successfully()
        {
            // Arrange
            var userId = "1";
            var user = new User { Id = userId, IsActive = true, FullName = "Original Name", Email = "original@example.com", PhoneNumber = "0274810934", Password = "hashedPassword" };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
            _userRepository.SoftDeleteAsync(userId).Returns(true);

            // Act
            var result = await _sut.DeleteUserAsync(userId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(200);
                result.Data.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task DeleteUserAsync_Should_Return_NotFound_When_User_Is_Not_Found()
        {
            // Arrange
            var userId = "1";
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns((User)null);

            // Act
            var result = await _sut.DeleteUserAsync(userId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(404);
                result.Message.Should().Be("User not found");
            }
        }

        [Fact]
        public async Task DeleteUserAsync_Should_Return_FailedDependency_When_Deletion_Fails()
        {
            // Arrange
            var userId = "1";
            var user = new User { Id = userId, IsActive = true, FullName = "Original Name", Email = "original@example.com", PhoneNumber = "0274810934", Password = "hashedPassword" };
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>()).Returns(user);
            _userRepository.SoftDeleteAsync(userId).Returns(false);

            // Act
            var result = await _sut.DeleteUserAsync(userId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(424);
                result.Message.Should().Be("Error occurred while deleting user");
            }
        }

        [Fact]
        public async Task DeleteUserAsync_Should_Return_InternalServerError_When_Exception_Occurs()
        {
            // Arrange
            var userId = "1";
            _userRepository.FindAsync(Arg.Any<Expression<Func<User, bool>>>())
                .Returns(Task.FromException<User>(new Exception("Exception occurred")));

            // Act
            var result = await _sut.DeleteUserAsync(userId);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(500);
                result.Message.Should().Be("Something Bad Happened. Please try again later.");
            }
        }
        
        #endregion
        
        #region GetAllUsersAsync
        //   [Fact]
        // public async Task GetAllUsersAsync_Should_Return_Ok_When_Users_Are_Found()
        // {
        //     // Arrange
        //     var filter = new BaseFilter { Search = "Test", CreatedAt = DateTime.Today};
        //     var users = new List<User>
        //     {
        //         new User { Id = "1", FullName = "Test User", Email = "test@example.com", IsActive = true, CreatedAt = DateTime.Now , PhoneNumber = "0274810934", Password = "hashedPassword"},
        //     };
        //     _userRepository.AsQueryableAsync().Returns(users.ToList());
        //
        //     // Act
        //     var result = await _sut.GetAllUsersAsync(filter);
        //
        //     // Assert
        //     using (new AssertionScope())
        //     {
        //         result.Should().NotBeNull();
        //         result.Code.Should().Be(200);
        //         result.Data.Should().NotBeNull();
        //         result.Data.TotalCount.Should().Be(1);
        //     }
        // }

        [Fact]
        public async Task GetAllUsersAsync_Should_Return_InternalServerError_When_Exception_Occurs()
        {
            // Arrange
            var filter = new BaseFilter { PageNumber = 1, PageSize = 10, Search = "Test" };
            _userRepository.AsQueryable().Returns(_ => throw new Exception("Exception occurred"));

            // Act
            var result = await _sut.GetAllUsersAsync(filter);

            // Assert
            using (new AssertionScope())
            {
                result.Should().NotBeNull();
                result.Code.Should().Be(500);
                result.Message.Should().Be("Something Bad Happened. Please try again later.");
            }
        }
        #endregion
    }
}

    
