using MediatR;
using FileMetadataAPI.DataContext;
using AutoMapper;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using FileMetadataAPI.Commands;
using File = FileMetadataAPI.Models.File;

namespace FileMetadataAPI.Handlers
{
    internal class ShareFileCommandHandler(
        ApplicationDbContext context,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<ShareFileCommand, Unit>
    {
        public async Task<Unit> Handle(ShareFileCommand request, CancellationToken cancellationToken)
        {
            // Get the current user's ID from claims
            var userId = int.Parse(httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new Exception("User ID not found."));

            // Find the file, ensuring it belongs to the current user
            var file = await context.Files
                .Include(f => f.FileShares)
                .FirstOrDefaultAsync(f => f.Id == request.FileId && f.OwnerId == userId, cancellationToken)
                ?? throw new Exception("File not found or authorization error.");

            // Update file visibility
            file.Visibility = Enum.Parse<Models.Visibility>(request.Visibility);

            // Clear existing shares and add new ones
            file.FileShares.Clear();
            file.FileShares.AddRange(mapper.Map<List<Models.FileShare>>(request.FileShares));

            // Save changes to the database
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}