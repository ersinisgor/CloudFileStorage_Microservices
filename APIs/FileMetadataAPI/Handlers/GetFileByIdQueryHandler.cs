using MediatR;
using FileMetadataAPI.DataContext;
using FileMetadataAPI.DTOs;
using AutoMapper;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using FileMetadataAPI.Queries;
using Microsoft.Extensions.Logging;
using FileMetadataAPI.Models;

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
            // Kullanıcının kimliğini al
            var userIdClaim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new ForbiddenException("User ID claim not found.");
            }
            var userId = int.Parse(userIdClaim.Value);

            var file = await context.Files
                .Include(f => f.FileShares)
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException("File not found.");

            // İzin kontrolü
            if (file.Visibility != Visibility.Public && file.OwnerId != userId && !file.FileShares.Any(fs => fs.UserId == userId && (fs.Permission == "Read" || fs.Permission == "Edit")))
            {
                throw new ForbiddenException("You do not have permission to access this file.");
            }

            return mapper.Map<FileDTO>(file);
        }
    }
}