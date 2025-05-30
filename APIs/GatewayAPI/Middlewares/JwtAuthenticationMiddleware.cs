using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace GatewayAPI.Middlewares
{
    public class JwtAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<JwtAuthenticationMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Authorization header missing or invalid");
                logger.LogWarning("No valid Authorization header found for request to {Path}", context.Request.Path);
                return;
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                logger.LogInformation("JWT Token validated successfully for request to {Path}", context.Request.Path);

                context.Request.Headers["Authorization"] = $"Bearer {token}";
                await next(context);
            }
            catch (SecurityTokenException ex)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Invalid token");
                logger.LogError("JWT validation failed: {Error}", ex.Message);
            }
        }
    }
}
