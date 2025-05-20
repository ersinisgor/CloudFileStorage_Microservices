using MediatR;
using AuthenticationAPI.DTOs;
using System.ComponentModel.DataAnnotations;

namespace AuthenticationAPI.Commands
{
    public class RegisterCommand : IRequest<UserDTO>
    {
        [Required]
        public string Name { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}