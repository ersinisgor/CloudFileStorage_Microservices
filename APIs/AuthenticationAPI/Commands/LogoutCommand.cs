using MediatR;

namespace AuthenticationAPI.Commands
{
    public class LogoutCommand : IRequest<Unit> { }
}