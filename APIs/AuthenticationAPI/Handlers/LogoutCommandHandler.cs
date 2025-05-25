using MediatR;
using AuthenticationAPI.DataContext;
using Microsoft.EntityFrameworkCore;
using AuthenticationAPI.Commands;

namespace AuthenticationAPI.Handlers
{
    internal class LogoutCommandHandler(ApplicationDbContext context) : IRequestHandler<LogoutCommand, Unit>
    {
        public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var user = await context.Users
                .SingleOrDefaultAsync(u => u.Id.ToString() == request.UserId, cancellationToken);

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