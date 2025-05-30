using MediatR;
using FileMetadataAPI.DataContext;
using AutoMapper;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using FileMetadataAPI.Commands;

namespace FileMetadataAPI.Handlers
{
    internal class ShareFileCommandHandler(
        ApplicationDbContext context,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<ShareFileCommand, Unit>
    {
        public async Task<Unit> Handle(ShareFileCommand request, CancellationToken cancellationToken)
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
                .Include(f => f.FileShares)
                .FirstOrDefaultAsync(f => f.Id == request.FileId, cancellationToken)
                ?? throw new NotFoundException("File not found.");

            if (file.OwnerId != userId)
            {
                throw new ForbiddenException("Only the file owner can share this file.");
            }

            file.Visibility = Enum.Parse<Models.Visibility>(request.Visibility);
            file.FileShares.Clear();
            file.FileShares.AddRange(mapper.Map<List<Models.FileShare>>(request.FileShares));

            await context.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}