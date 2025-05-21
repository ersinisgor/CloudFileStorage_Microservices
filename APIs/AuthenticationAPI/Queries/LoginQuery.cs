using MediatR;
using AuthenticationAPI.DTOs;

namespace AuthenticationAPI.Queries
{
    internal class LoginQuery : IRequest<AuthResult>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}