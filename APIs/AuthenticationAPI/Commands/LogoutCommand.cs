using MediatR;

namespace AuthenticationAPI.Commands
{
    public class LogoutCommand : IRequest<Unit>
    {
        public string UserId { get; set; }
        public string Token { get; set; }
    }
}