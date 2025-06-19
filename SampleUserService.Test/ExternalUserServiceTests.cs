using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using SampleUserService.Models;
using SampleUserService.Services;

namespace SampleUserService.Test
{
    public class ExternalUserServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<ILogger<ExternalUserService>> _loggerMock;
        private readonly IMemoryCache _memoryCache;
        private readonly ApiSettings _apiSettings;

        public ExternalUserServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _loggerMock = new Mock<ILogger<ExternalUserService>>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _apiSettings = new ApiSettings { BaseUrl = "https://reqres.in/api/" };
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenApiCallSucceeds()
        {
            // Arrange
            var expectedUser = new ApiUser { Id = 1, Email = "test@example.com", FirstName = "John", LastName = "Doe" };
            var apiResponse = new ApiResponse<ApiUser> { Data = expectedUser };

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(apiResponse), Encoding.UTF8, "application/json")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains("users/1")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var service = new ExternalUserService(
                _httpClient,
                Options.Create(_apiSettings),
                _loggerMock.Object,
                _memoryCache);

            // Act
            var result = await service.GetUserByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedUser.Id, result.Id);
            Assert.Equal(expectedUser.Email, result.Email);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenUserNotFound()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains("users/999")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var service = new ExternalUserService(
                _httpClient,
                Options.Create(_apiSettings),
                _loggerMock.Object,
                _memoryCache);

            // Act
            var result = await service.GetUserByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllUsers_HandlingPagination()
        {
            // Arrange
            var page1Users = new UserListResponse
            {
                Users = new List<ApiUser>
            {
                new() { Id = 1, Email = "user1@example.com" },
                new() { Id = 2, Email = "user2@example.com" }
            }
            };

            var page2Users = new UserListResponse
            {
                Users = new List<ApiUser>
            {
                new() { Id = 3, Email = "user3@example.com" },
                new() { Id = 4, Email = "user4@example.com" }
            }
            };

            var page1Response = new ApiResponse<UserListResponse>
            {
                Data = page1Users,
                Page = 1,
                TotalPages = 2
            };

            var page2Response = new ApiResponse<UserListResponse>
            {
                Data = page2Users,
                Page = 2,
                TotalPages = 2
            };

            _httpMessageHandlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains("users?page=1")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(page1Response), Encoding.UTF8, "application/json")
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(page2Response), Encoding.UTF8, "application/json")
                });

            var service = new ExternalUserService(
                _httpClient,
                Options.Create(_apiSettings),
                _loggerMock.Object,
                _memoryCache);

            // Act
            var result = await service.GetAllUsersAsync();

            // Assert
            Assert.Equal(4, result.Count());
        }

        [Fact]
        public async Task GetUserByIdAsync_UsesCache_WhenAvailable()
        {
            // Arrange
            var cachedUser = new ApiUser { Id = 5, Email = "cached@example.com" };
            _memoryCache.Set($"user_5", cachedUser);

            var service = new ExternalUserService(
                _httpClient,
                Options.Create(_apiSettings),
                _loggerMock.Object,
                _memoryCache);

            // Act
            var result = await service.GetUserByIdAsync(5);

            // Assert
            Assert.Equal(cachedUser.Email, result?.Email);
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
