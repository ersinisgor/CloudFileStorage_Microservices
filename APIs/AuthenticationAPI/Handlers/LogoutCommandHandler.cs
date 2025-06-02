using MediatR;
using AuthenticationAPI.DataContext;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AuthenticationAPI.Commands;

namespace AuthenticationAPI.Handlers
{
    internal class LogoutCommandHandler(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor) : IRequestHandler<LogoutCommand, Unit>
    {
        public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                throw new InvalidOperationException("HttpContext is not available");
            }

            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var userId = int.Parse(userIdClaim.Value);
            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}