using MediatR;
using AuthenticationAPI.DTOs;

namespace AuthenticationAPI.Commands
{
    internal class RefreshTokenCommand : IRequest<AuthResult>
    {
        public string RefreshToken { get; set; }
    }
}