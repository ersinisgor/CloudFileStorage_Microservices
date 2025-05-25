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
    // Custom exception for not found scenarios
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    // Custom exception for authorization errors
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
            var userIdClaim = httpContextAccessor.HttpContext.User.FindFirst("nameid")
                ?? throw new Exception("User ID not found in claims.");

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                logger.LogError("Invalid User ID format: {UserId}", userIdClaim.Value);
                throw new Exception("Invalid User ID format.");
            }

            var file = await context.Files
                .Include(f => f.FileShares)
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException("File not found.");

            if (file.OwnerId != userId && file.Visibility != Models.Visibility.Public &&
                !file.FileShares.Any(fs => fs.UserId == userId && fs.Permission == "Read"))
            {
                throw new ForbiddenException("Authorization error: You do not have permission to access this file.");
            }

            return mapper.Map<FileDTO>(file);
        }
    }
}