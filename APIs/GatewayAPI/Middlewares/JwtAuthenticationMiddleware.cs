using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GatewayAPI.Middlewares
{
    public class JwtAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtAuthenticationMiddleware> _logger;
        private readonly string[] _anonymousPaths = ["/api/auth/login", "/api/auth/register"];

        public JwtAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<JwtAuthenticationMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            // Anonim endpoint'ler için JWT kontrolünü atla
            if (_anonymousPaths.Any(p => path.StartsWith(p.ToLowerInvariant())))
            {
                _logger.LogInformation("Skipping JWT validation for anonymous path: {Path}", path);
                await _next(context);
                return;
            }

            // Authorization başlığını kontrol et
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("No valid Authorization header found for request to {Path}", path);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };

                // Token'ı doğrula ve claim'leri al
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                context.User = principal; // HttpContext.User'ı doldur

                _logger.LogInformation("JWT Token validated successfully for request to {Path}", path);

                // Token'ı istek başlığında tut
                context.Request.Headers["Authorization"] = $"Bearer {token}";
                await _next(context);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, "JWT validation failed for request to {Path}", path);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during JWT validation for request to {Path}", path);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return;
            }
        }
    }
}