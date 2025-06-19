using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SampleUserService.Models;
using SampleUserService.Services;

namespace SampleUserService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExternalUserService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiSettings>(configuration.GetSection(ApiSettings.SectionName));

            services.AddHttpClient<IExternalUserService, ExternalUserService>(client =>
            {
                var apiSettings = configuration.GetSection(ApiSettings.SectionName).Get<ApiSettings>();
                client.BaseAddress = new Uri(apiSettings?.BaseUrl ?? "https://reqres.in/api/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            services.AddMemoryCache();
            services.AddScoped<IExternalUserService, ExternalUserService>();

            return services;
        }
    }
}
