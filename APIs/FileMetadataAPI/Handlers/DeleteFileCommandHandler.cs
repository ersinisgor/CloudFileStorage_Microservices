using MediatR;
using FileMetadataAPI.DataContext;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using FileMetadataAPI.Commands;

namespace FileMetadataAPI.Handlers
{
    internal class DeleteFileCommandHandler(
           ApplicationDbContext context,
           IHttpContextAccessor httpContextAccessor,
           ILogger<DeleteFileCommandHandler> logger) : IRequestHandler<DeleteFileCommand>
    {
        public async Task Handle(DeleteFileCommand request, CancellationToken cancellationToken)
        {
            if (httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated != true)
            {
                logger.LogWarning("Unauthorized access attempt to delete file with ID {FileId}.", request.Id);
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var userIdClaim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                logger.LogWarning("Forbidden access: User ID claim not found for file deletion attempt with ID {FileId}.", request.Id);
                throw new ForbiddenException("User ID claim not found.");
            }
            var userId = int.Parse(userIdClaim.Value);

            var file = await context.Files
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException("File not found.");

            var roleClaim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (file.OwnerId != userId && roleClaim != "admin")
            {
                logger.LogWarning("Forbidden access: User {UserId} attempted to delete file {FileId} without proper permissions.", userId, request.Id);
                throw new ForbiddenException("Only the file owner or admin can delete this file.");
            }

            logger.LogInformation("User {UserId} is deleting file {FileId}.", userId, request.Id);
            context.Files.Remove(file);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("File {FileId} successfully deleted by user {UserId}.", request.Id, userId);
        }
    }
}