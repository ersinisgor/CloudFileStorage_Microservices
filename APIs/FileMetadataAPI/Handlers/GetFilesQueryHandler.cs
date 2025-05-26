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
            // Önce JWT token'dan claim'leri kontrol et
            var userIdFromClaim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Eğer claim'de yoksa, Gateway'den gelen header'ı kontrol et
            var userIdFromHeader = httpContextAccessor.HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault();
            var currentUserIdHeader = httpContextAccessor.HttpContext?.Request.Headers["X-Current-User-Id"].FirstOrDefault();

            var userIdString = userIdFromClaim ?? userIdFromHeader ?? currentUserIdHeader;

            if (string.IsNullOrEmpty(userIdString))
            {
                logger.LogError("User ID not found in claims or headers");
                throw new UnauthorizedAccessException("User ID not found in request");
            }

            if (!int.TryParse(userIdString, out var userId))
            {
                logger.LogError("Invalid User ID format: {UserId}", userIdString);
                throw new ArgumentException("Invalid User ID format");
            }

            // Debug için tüm user bilgilerini logla
            var userEmail = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value ??
                           httpContextAccessor.HttpContext?.Request.Headers["X-User-Email"].FirstOrDefault();
            var userName = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ??
                          httpContextAccessor.HttpContext?.Request.Headers["X-User-Name"].FirstOrDefault();
            var userRole = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value ??
                          httpContextAccessor.HttpContext?.Request.Headers["X-User-Role"].FirstOrDefault();

            logger.LogInformation("Fetching files for user - ID: {UserId}, Email: {UserEmail}, Name: {UserName}, Role: {UserRole}",
                userId, userEmail, userName, userRole);

            try
            {
                var files = await context.Files
                    .Include(f => f.FileShares)
                    .Where(f => f.OwnerId == userId ||
                                f.Visibility == Models.Visibility.Public ||
                                (f.Visibility == Models.Visibility.Shared &&
                                 f.FileShares.Any(fs => fs.UserId == userId)))
                    .ToListAsync(cancellationToken);

                logger.LogInformation("Found {FileCount} files for user {UserId}", files.Count, userId);

                var result = mapper.Map<List<FileDTO>>(files);

                // Her dosya için sahiplik bilgisini ekle
                foreach (var fileDto in result)
                {
                    //var originalFile = files.First(f => f.Id == fileDto.Id);
                    //fileDto.IsOwner = originalFile.OwnerId == userId;
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