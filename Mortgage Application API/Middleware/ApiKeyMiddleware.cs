namespace Mortage_Application_API.Middleware
{
    using System.Security.Cryptography;
    using System.Text;

    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _apiKey;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _apiKey = configuration["ApiKey"] ?? throw new InvalidOperationException("API key is not configured");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if request contains an API key
            if (!context.Request.Headers.TryGetValue("X-Api-Key", out var providedKey))
            {
                context.Response.StatusCode = 401;
                return;
            }

            // Check if API key is valid
            var expected = Encoding.UTF8.GetBytes(_apiKey);
            var actual = Encoding.UTF8.GetBytes(providedKey.ToString());

            if (!CryptographicOperations.FixedTimeEquals(
                expected, actual))
            {
                context.Response.StatusCode = 401;
                return;
            }

            // Continue to controller
            await _next(context);
        }
    }
}
