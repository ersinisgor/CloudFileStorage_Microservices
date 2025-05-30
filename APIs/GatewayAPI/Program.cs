using GatewayAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.SetMinimumLevel(LogLevel.Debug);
});

// YARP configuration
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// JWT Authentication configuration (middleware için)
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Authorization policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AuthenticatedUser", policy =>
        policy.RequireAuthenticatedUser());
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
app.UseMiddleware<JwtAuthenticationMiddleware>();
app.MapReverseProxy();

await app.RunAsync();