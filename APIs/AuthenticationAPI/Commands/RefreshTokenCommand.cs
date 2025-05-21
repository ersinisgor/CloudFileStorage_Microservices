using MediatR;
using AuthenticationAPI.DTOs;

namespace AuthenticationAPI.Commands
{
    public class RefreshTokenCommand : IRequest<AuthResult>
    {
        public string RefreshToken { get; set; }
    }
}