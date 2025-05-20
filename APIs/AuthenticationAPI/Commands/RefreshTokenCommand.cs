using MediatR;
using AuthenticationAPI.DTOs;
using System.ComponentModel.DataAnnotations;

namespace AuthenticationAPI.Commands
{
    public class RefreshTokenCommand : IRequest<AuthResult>
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}