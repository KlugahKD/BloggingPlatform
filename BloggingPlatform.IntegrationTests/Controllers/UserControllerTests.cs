using System.Text;
using System.Text.Json;
using BloggingPlatform.Business.Models.Requests;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace BloggingPlatform.IntegrationTests.Controllers
{
    public class UsersControllerTests(BloggingPlatformApplicationFactory<Program> factory)
        : IClassFixture<BloggingPlatformApplicationFactory<Program>>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task GetAllUsers_Should_Return_Ok()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/users");

            // Assert
            response.EnsureSuccessStatusCode();
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetUserById_Should_Return_Ok_When_User_Exists()
        {
            // Arrange
            var userId = Guid.NewGuid(); 

            // Act
            var response = await _client.GetAsync($"/api/v1/users/{userId}");

            // Assert
            response.EnsureSuccessStatusCode(); 
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetUserById_Should_Return_NotFound_When_User_Does_Not_Exist()
        {
            // Arrange
            var userId = ""; 

            // Act
            var response = await _client.GetAsync($"/api/v1/users/{userId}");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AddUser_Should_Return_Created()
        {
            // Arrange
            var request = new AddUserRequest
            {
                FullName = "Test User",
                PhoneNumber = "0274810934",
                Password = "password",
            };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/users", content);

            // Assert
            response.EnsureSuccessStatusCode(); 
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        }

        [Fact]
        public async Task UpdateUser_Should_Return_Ok()
        {
            // Arrange
            var userId = Guid.NewGuid(); 
            var request = new UpdateUserRequest
            {
                FullName = "New Test User",
            };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PutAsync($"/api/v1/users/{userId}", content);

            // Assert
            response.EnsureSuccessStatusCode(); 
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task DeleteUser_Should_Return_Ok()
        {
            // Arrange
            var userId = Guid.NewGuid(); 

            // Act
            var response = await _client.DeleteAsync($"/api/v1/users/{userId}");

            // Assert
            response.EnsureSuccessStatusCode(); 
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task Login_Should_Return_Ok()
        {
            // Arrange
            var request = new LoginRequest
            {
                PhoneNumber = "0274810934",
                Password = "password",
            };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/users/login", content);

            // Assert
            response.EnsureSuccessStatusCode(); 
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}