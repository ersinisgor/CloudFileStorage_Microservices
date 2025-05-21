using MediatR;
using AuthenticationAPI.DTOs;

namespace AuthenticationAPI.Commands
{
    public class RegisterCommand : IRequest<UserDTO>
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}