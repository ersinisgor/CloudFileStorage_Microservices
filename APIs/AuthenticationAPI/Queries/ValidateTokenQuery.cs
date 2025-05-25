using MediatR;

namespace AuthenticationAPI.Queries
{
    public class ValidateTokenQuery : IRequest<bool>
    {
        public string Token { get; set; }
    }
}