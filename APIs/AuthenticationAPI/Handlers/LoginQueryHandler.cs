using MediatR;
using AuthenticationAPI.DataContext;
using AuthenticationAPI.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthenticationAPI.Queries;
using AuthenticationAPI.Validators;
using FluentValidation;

namespace AuthenticationAPI.Handlers
{
    internal class LoginQueryHandler(ApplicationDbContext context, IConfiguration configuration) : IRequestHandler<LoginQuery, AuthResult>
    {
        public async Task<AuthResult> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var user = context.Users.SingleOrDefault(u => u.Email == request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new Exception("Invalid credentials");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = configuration["Jwt:Issuer"],
                Audience = configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = Guid.NewGuid().ToString();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await context.SaveChangesAsync(cancellationToken);

            var authResult =  new AuthResult
            {
                Token = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken
            };

            var validator = new AuthResultValidator();
            var validationResult = await validator.ValidateAsync(authResult, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            return authResult;
        }
    }
}