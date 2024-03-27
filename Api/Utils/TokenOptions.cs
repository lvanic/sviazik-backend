using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Api.Utils
{
    public static class TokenOptions
    {
        private static IConfiguration _configuration;
        public static string ISSUER = "https://localhost:3001";
        public static string AUDIENCE { get; set; }
        private static string KEY { get; set; }
        public static int LIFETIME { get; set; }
        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
            ISSUER = configuration.GetRequiredSection("URLS").Value.Split(';')[0] ?? throw new Exception("Cant't get URL of application");
            AUDIENCE = _configuration["ClientHost"] ?? throw new Exception("ClientHost is not define in appsettings.json");
            KEY = _configuration["Jwt:Secret"] ?? throw new Exception("Key is not define in appsettings.json");
            LIFETIME = _configuration.GetValue<Int32>("Jwt:ExpirationInMinutes");
        }
        public static SymmetricSecurityKey GetSymmeetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }
}
