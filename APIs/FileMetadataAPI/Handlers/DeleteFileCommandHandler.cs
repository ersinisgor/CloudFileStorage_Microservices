using MediatR;
using FileMetadataAPI.DataContext;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using FileMetadataAPI.Commands;

namespace FileMetadataAPI.Handlers
{
    internal class DeleteFileCommandHandler(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<DeleteFileCommand>
    {
        public async Task Handle(DeleteFileCommand request, CancellationToken cancellationToken)
        {
            if (httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated != true)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var userIdClaim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new ForbiddenException("User ID claim not found.");
            }
            var userId = int.Parse(userIdClaim.Value);

            var file = await context.Files
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException("File not found.");

            // RBAC and ABAC
            var roleClaim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (file.OwnerId != userId && roleClaim != "admin")
            {
                throw new ForbiddenException("Only the file owner or admin can delete this file.");
            }

            context.Files.Remove(file);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}