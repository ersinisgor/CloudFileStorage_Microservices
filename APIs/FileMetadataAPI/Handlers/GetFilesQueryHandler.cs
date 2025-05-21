using MediatR;
using FileMetadataAPI.DataContext;
using FileMetadataAPI.DTOs;
using AutoMapper;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using FileMetadataAPI.Queries;

namespace FileMetadataAPI.Handlers
{
    internal class GetFilesQueryHandler(ApplicationDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetFilesQuery, List<FileDTO>>
    {
        public async Task<List<FileDTO>> Handle(GetFilesQuery request, CancellationToken cancellationToken)
        {
            var userId = int.Parse(httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new Exception("User ID not found."));

            var files = await context.Files
                .Include(f => f.FileShares)
                .Where(f => f.OwnerId == userId || f.Visibility == Models.Visibility.Public ||
                            f.FileShares.Any(fs => fs.UserId == userId))
                .ToListAsync(cancellationToken);

            return mapper.Map<List<FileDTO>>(files);
        }
    }
}