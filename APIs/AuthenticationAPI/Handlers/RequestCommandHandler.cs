using MediatR;
using AuthenticationAPI.DataContext;
using AuthenticationAPI.Models;
using AuthenticationAPI.DTOs;
using AuthenticationAPI.Commands;

namespace AuthenticationAPI.Handlers
{
    public class RegisterCommandHandler(ApplicationDbContext context) : IRequestHandler<RegisterCommand, UserDTO>
    {
        public async Task<UserDTO> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            if (context.Users.Any(u => u.Email == request.Email))
                throw new Exception("User already existing");

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "User"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync(cancellationToken);

            return new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }
    }
}