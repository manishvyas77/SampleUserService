using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleUserService.Models
{
    public class ApiSettings
    {
        public const string SectionName = "ApiSettings";

        public string BaseUrl { get; set; } = "https://reqres.in/api/";
        public int CacheExpirationMinutes { get; set; } = 5;
    }
}
