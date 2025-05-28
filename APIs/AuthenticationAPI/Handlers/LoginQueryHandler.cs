using MediatR;
using AuthenticationAPI.DataContext;
using AuthenticationAPI.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using AuthenticationAPI.Queries;

namespace AuthenticationAPI.Handlers
{
    internal class LoginQueryHandler(ApplicationDbContext context, IConfiguration configuration, ILogger<LoginQueryHandler> logger, IMapper mapper) : IRequestHandler<LoginQuery, AuthResult>
    {
        public async Task<AuthResult> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing login for email: {Email}", request.Email);

            var user = context.Users.SingleOrDefault(u => u.Email == request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                logger.LogWarning("Login failed for email: {Email}. Invalid credentials.", request.Email);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured."));
            var expires = DateTime.UtcNow.AddDays(double.Parse(configuration["Jwt:Expires"]));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name ?? string.Empty),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = expires,
                Issuer = configuration["Jwt:Issuer"],
                Audience = configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            var refreshToken = Guid.NewGuid().ToString();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(double.Parse(configuration["Jwt:RefreshTokenExpiry"]));
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = refreshTokenExpiry;
            await context.SaveChangesAsync(cancellationToken);

            var authResult = new AuthResult
            {
                Token = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken,
                User = mapper.Map<UserInfo>(user)
            };

            logger.LogInformation("Login successful for email: {Email}", request.Email);
            return authResult;
        }
    }
}