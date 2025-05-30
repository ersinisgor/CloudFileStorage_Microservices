using MediatR;
using FileMetadataAPI.DataContext;
using FileMetadataAPI.DTOs;
using AutoMapper;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using FileMetadataAPI.Queries;
using Microsoft.Extensions.Logging;

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

            var userEmail = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            var userName = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            var userRole = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            logger.LogInformation("Fetching files for user - ID: {UserId}, Email: {UserEmail}, Name: {UserName}, Role: {UserRole}",
                userId, userEmail, userName, userRole);

            try
            {
                var files = await context.Files
                    .Include(f => f.FileShares)
                    .Where(f => f.OwnerId == userId ||
                                f.Visibility == Models.Visibility.Public ||
                                (f.Visibility == Models.Visibility.Shared &&
                                 f.FileShares.Any(fs => fs.UserId == userId)) ||
                                httpContextAccessor.HttpContext.User.IsInRole("admin"))
                    .ToListAsync(cancellationToken);

                logger.LogInformation("Found {FileCount} files for user {UserId}", files.Count, userId);

                var result = mapper.Map<List<FileDTO>>(files);
                foreach (var fileDto in result)
                {
                    fileDto.IsOwner = fileDto.OwnerId == userId;
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching files for user {UserId}", userId);
                throw;
            }
        }
    }
}