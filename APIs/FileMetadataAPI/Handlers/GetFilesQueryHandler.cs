using MediatR;
using FileMetadataAPI.DataContext;
using FileMetadataAPI.DTOs;
using AutoMapper;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using FileMetadataAPI.Queries;

namespace FileMetadataAPI.Handlers
{
    internal class GetFilesQueryHandler(
        ApplicationDbContext context,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogger<GetFilesQueryHandler> logger) : IRequestHandler<GetFilesQuery, List<FileDTO>>
    {
        public async Task<List<FileDTO>> Handle(GetFilesQuery request, CancellationToken cancellationToken)
        {

            var userIdClaim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)
                ?? throw new Exception("User ID not found in claims.");

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                logger.LogError("Invalid User ID format: {UserId}", userIdClaim.Value);
                throw new Exception("Invalid User ID format.");
            }

            logger.LogInformation("Fetching files for user ID: {UserId}", userId);

            var files = await context.Files
                .Include(f => f.FileShares)
                .Where(f => f.OwnerId == userId || f.Visibility == Models.Visibility.Public ||
                            f.FileShares.Any(fs => fs.UserId == userId))
                .ToListAsync(cancellationToken);

            return mapper.Map<List<FileDTO>>(files);
        }
    }
}