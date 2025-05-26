using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});

// JWT Authentication configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero
        };
        // Log token validation errors
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("JWT Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var userEmail = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                logger.LogInformation("JWT Token validated successfully for user: {UserId} ({Email})", userId, userEmail);
                return Task.CompletedTask;
            }
        };
    });

// Authorization policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
});

// YARP configuration
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(async context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

            // Forward Authorization header to downstream services
            if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                if (authHeader.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeader.ToString().Substring("Bearer ".Length).Trim();
                    context.ProxyRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    logger.LogDebug("Forwarding Authorization header with token: {TokenPrefix}...", token[..Math.Min(10, token.Length)]);
                }
            }
            else
            {
                logger.LogWarning("No Authorization header found in request to {Path}", context.HttpContext.Request.Path);
            }

            // Forward all user claims as headers to downstream services
            if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var claims = context.HttpContext.User.Claims.ToList();

                foreach (var claim in claims)
                {
                    var headerName = claim.Type switch
                    {
                        System.Security.Claims.ClaimTypes.NameIdentifier => "X-User-Id",
                        System.Security.Claims.ClaimTypes.Name => "X-User-Name",
                        System.Security.Claims.ClaimTypes.Email => "X-User-Email",
                        System.Security.Claims.ClaimTypes.Role => "X-User-Role",
                        _ => $"X-Claim-{claim.Type.Replace("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/", "").Replace("http://schemas.microsoft.com/ws/2008/06/identity/claims/", "")}"
                    };

                    if (!string.IsNullOrEmpty(claim.Value))
                    {
                        // Mevcut header'ý kaldýr ve yenisini ekle
                        context.ProxyRequest.Headers.Remove(headerName);
                        context.ProxyRequest.Headers.Add(headerName, claim.Value);

                        logger.LogDebug("Forwarding claim {ClaimType} as header {HeaderName}: {ClaimValue}",
                            claim.Type, headerName, claim.Value);
                    }
                }

                // Özel header'lar ekle
                var userId = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    context.ProxyRequest.Headers.Remove("X-Current-User-Id");
                    context.ProxyRequest.Headers.Add("X-Current-User-Id", userId);
                }
            }
            else
            {
                logger.LogDebug("User is not authenticated for request to {Path}", context.HttpContext.Request.Path);
            }

            await Task.CompletedTask;
        });
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC", policy =>
    {
        policy.WithOrigins("https://localhost:5004")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("AllowMVC");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();

app.Run();