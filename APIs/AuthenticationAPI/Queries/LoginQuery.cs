using MediatR;
using AuthenticationAPI.DTOs;
using System.ComponentModel.DataAnnotations;

namespace AuthenticationAPI.Queries
{
    public class LoginQuery : IRequest<AuthResult>
    {
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}