﻿using MediatR;
using AuthenticationAPI.DataContext;
using AuthenticationAPI.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthenticationAPI.Commands;
using AutoMapper;

namespace AuthenticationAPI.Handlers
{
    internal class RefreshTokenCommandHandler(ApplicationDbContext context, IConfiguration configuration, IMapper mapper) : IRequestHandler<RefreshTokenCommand, AuthResult>
    {
        public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var user = context.Users.SingleOrDefault(u => u.RefreshToken == request.RefreshToken && u.RefreshTokenExpiry > DateTime.UtcNow);
            if (user == null)
                throw new InvalidOperationException("Invalid or expired refresh token");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]);
            var expires = DateTime.UtcNow.AddDays(double.Parse(configuration["Jwt:Expires"]));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = expires,
                Issuer = configuration["Jwt:Issuer"],
                Audience = configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            var newRefreshToken = Guid.NewGuid().ToString();
            var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(double.Parse(configuration["Jwt:RefreshTokenExpiry"]));
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = newRefreshTokenExpiry;
            await context.SaveChangesAsync(cancellationToken);

            return new AuthResult
            {
                Token = tokenHandler.WriteToken(token),
                RefreshToken = newRefreshToken,
                User = mapper.Map<UserInfo>(user)
            };
        }
    }
}