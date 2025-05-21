using MediatR;
using FileMetadataAPI.DataContext;
using FileMetadataAPI.DTOs;
using AutoMapper;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using FileMetadataAPI.Queries;

namespace FileMetadataAPI.Handlers
{
    internal class GetFileByIdQueryHandler(
        ApplicationDbContext context,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetFileByIdQuery, FileDTO>
    {
        public async Task<FileDTO> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
        {
            var userId = int.Parse(httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new Exception("User ID not found."));

            var file = await context.Files
                .Include(f => f.FileShares)
                .FirstOrDefaultAsync(f => f.Id == request.Id &&
                    (f.OwnerId == userId || f.Visibility == Models.Visibility.Public ||
                     f.FileShares.Any(fs => fs.UserId == userId)), cancellationToken)
                ?? throw new Exception("File not found or authorization error.");

            return mapper.Map<FileDTO>(file);
        }
    }
}