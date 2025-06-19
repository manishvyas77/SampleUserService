using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SampleUserService.Models
{

    public class RootApiResponse
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("per_page")]
        public int PerPage { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("data")]
        public List<ApiUser> Data { get; set; }

        [JsonPropertyName("support")]
        public Support Support { get; set; }
    }
    public class Support
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
    public class ApiUser
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }
    }

    public class ApiResponse<T>
    {
        [JsonProperty(PropertyName = "data")]
        public T? Data { get; set; }

        [JsonProperty(PropertyName = "page")]
        public int Page { get; set; }

        [JsonProperty(PropertyName = "per_page")]
        public int PerPage { get; set; }

        [JsonProperty(PropertyName = "total")]
        public int Total { get; set; }

        [JsonProperty(PropertyName = "total_pages")]
        public int TotalPages { get; set; }

        [JsonProperty(PropertyName = "support")]
        public SupportInfo? Support { get; set; }
    }
    public class SupportInfo
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
