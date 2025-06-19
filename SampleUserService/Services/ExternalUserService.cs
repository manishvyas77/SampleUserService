using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SampleUserService.Models;

namespace SampleUserService.Services
{
    public interface IExternalUserService
    {
        Task<ApiUser?> GetUserByIdAsync(int userId);
        Task<IEnumerable<ApiUser>> GetAllUsersAsync();
    }
    public class ExternalUserService : IExternalUserService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExternalUserService> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly ApiSettings _apiSettings;

        public ExternalUserService(
            HttpClient httpClient,
            IOptions<ApiSettings> apiSettings,
            ILogger<ExternalUserService> logger,
            IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _memoryCache = memoryCache;
            _apiSettings = apiSettings.Value;
        }

        public async Task<ApiUser?> GetUserByIdAsync(int userId)
        {
            string cacheKey = $"user_{userId}";

            if (_memoryCache.TryGetValue(cacheKey, out ApiUser? cachedUser))
            {
                _logger.LogInformation($"Retrieved user {userId} from cache");
                return cachedUser;
            }

            try
            {
                var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}users/{userId}");

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"User with ID {userId} not found");
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<ApiUser>>(content);

                if (result?.Data == null)
                {
                    _logger.LogWarning($"No data returned for user ID {userId}");
                    return null;
                }

                // Cache the user with expiration
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(_apiSettings.CacheExpirationMinutes));

                _memoryCache.Set(cacheKey, result.Data, cacheOptions);

                return result.Data;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error fetching user with ID {userId}");
                throw new ExternalApiException($"Error fetching user with ID {userId}", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Error deserializing response for user ID {userId}");
                throw new ExternalApiException($"Invalid data received for user ID {userId}", ex);
            }
        }

        public async Task<IEnumerable<ApiUser>> GetAllUsersAsync()
        {
        //    string json = @"{
        //    ""page"":1,
        //    ""per_page"":6,
        //    ""total"":12,
        //    ""total_pages"":2,
        //    ""data"":[
        //        {""id"":1,""email"":""george.bluth@reqres.in"",""first_name"":""George"",""last_name"":""Bluth"",""avatar"":""https://reqres.in/img/faces/1-image.jpg""},
        //        {""id"":2,""email"":""janet.weaver@reqres.in"",""first_name"":""Janet"",""last_name"":""Weaver"",""avatar"":""https://reqres.in/img/faces/2-image.jpg""},
        //        {""id"":3,""email"":""emma.wong@reqres.in"",""first_name"":""Emma"",""last_name"":""Wong"",""avatar"":""https://reqres.in/img/faces/3-image.jpg""},
        //        {""id"":4,""email"":""eve.holt@reqres.in"",""first_name"":""Eve"",""last_name"":""Holt"",""avatar"":""https://reqres.in/img/faces/4-image.jpg""},
        //        {""id"":5,""email"":""charles.morris@reqres.in"",""first_name"":""Charles"",""last_name"":""Morris"",""avatar"":""https://reqres.in/img/faces/5-image.jpg""},
        //        {""id"":6,""email"":""tracey.ramos@reqres.in"",""first_name"":""Tracey"",""last_name"":""Ramos"",""avatar"":""https://reqres.in/img/faces/6-image.jpg""}
        //    ],
        //    ""support"": {
        //        ""url"": ""https://contentcaddy.io?utm_source=reqres&utm_medium=json&utm_campaign=referral"",
        //        ""text"": ""Tired of writing endless social media content? Let Content Caddy generate it for you.""
        //    }
        //}";

        //    RootApiResponse result11 = JsonSerializer.Deserialize<RootApiResponse>(json);

            //Console.WriteLine($"Total Users: {result.Data.Count}");
            //foreach (var user in result.Data)
            //{
            //    Console.WriteLine($"{user.FirstName} {user.LastName} - {user.Email}");
            //}

            string cacheKey = "all_users";

            if (_memoryCache.TryGetValue(cacheKey, out List<ApiUser>? cachedUsers))
            {
                _logger.LogInformation("Retrieved all users from cache");
                return cachedUsers ?? Enumerable.Empty<ApiUser>();
            }

            var allUsers = new List<ApiUser>();
            int currentPage = 1;
            int totalPages = 1;

            try
            {
                while (currentPage <= totalPages)
                {
                    var response = await _httpClient.GetAsync($"{_apiSettings.BaseUrl}users?page={currentPage}");
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    //var result = JsonSerializer.Deserialize<ApiResponse<UserListResponse>>(content);
                    RootApiResponse result = JsonSerializer.Deserialize<RootApiResponse>(content);


                    if (result?.Data == null)
                    {
                        _logger.LogWarning($"No users found on page {currentPage}");
                        break;
                    }

                    allUsers.AddRange(result.Data);
                    totalPages = result.TotalPages;
                    currentPage++;
                }

                // Cache all users with expiration
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(_apiSettings.CacheExpirationMinutes));

                _memoryCache.Set(cacheKey, allUsers, cacheOptions);

                return allUsers;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching all users");
                throw new ExternalApiException("Error fetching all users", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing users response");
                throw new ExternalApiException("Invalid data received for users", ex);
            }
        }
    }

    public class ExternalApiException : Exception
    {
        public ExternalApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
