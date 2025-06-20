﻿using MediatR;
using AuthenticationAPI.DataContext;
using AuthenticationAPI.Models;
using AuthenticationAPI.DTOs;
using AuthenticationAPI.Commands;
using AutoMapper;

namespace AuthenticationAPI.Handlers
{
    internal class RegisterCommandHandler(ApplicationDbContext context, IMapper mapper) : IRequestHandler<RegisterCommand, UserDTO>
    {
        public async Task<UserDTO> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            if (context.Users.Any(u => u.Email == request.Email))
                throw new InvalidOperationException("User with this email already exists");

            var user = mapper.Map<User>(request);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.Role = "User";

            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);

            return mapper.Map<UserDTO>(user);
        }
    }
}