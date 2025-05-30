using MediatR;
using FileMetadataAPI.DataContext;
using FileMetadataAPI.DTOs;
using AutoMapper;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using FileMetadataAPI.Queries;
using FileMetadataAPI.Models;

namespace FileMetadataAPI.Handlers
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }

    internal class GetFileByIdQueryHandler(
    ApplicationDbContext context,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor,
    ILogger<GetFileByIdQueryHandler> logger) : IRequestHandler<GetFileByIdQuery, FileDTO>
    {
        public async Task<FileDTO> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
        {
            if (httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated != true)
            {
                logger.LogWarning("Unauthorized access attempt.");
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var userIdClaim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                logger.LogWarning("Forbidden access: User ID claim not found.");
                throw new ForbiddenException("User ID claim not found.");
            }
            var userId = int.Parse(userIdClaim.Value);

            var file = await context.Files
                .Include(f => f.FileShares)
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException("File not found.");

            logger.LogInformation("File with ID {FileId} retrieved successfully.", request.Id);

            var roleClaim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            if (file.Visibility != Visibility.Public && file.OwnerId != userId && roleClaim != "admin" &&
                !file.FileShares.Any(fs => fs.UserId == userId && (fs.Permission == "Read" || fs.Permission == "Edit")))
            {
                logger.LogWarning("Forbidden access: User {UserId} does not have permission to access file {FileId}.", userId, request.Id);
                throw new ForbiddenException("You do not have permission to access this file.");
            }

            var fileDto = mapper.Map<FileDTO>(file);
            fileDto.IsOwner = file.OwnerId == userId;
            return fileDto;
        }
    }
}