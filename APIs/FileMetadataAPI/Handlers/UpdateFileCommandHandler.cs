using MediatR;
using FileMetadataAPI.DataContext;
using FileMetadataAPI.DTOs;
using AutoMapper;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using FileMetadataAPI.Commands;
using FileShare = FileMetadataAPI.Models.FileShare;

namespace FileMetadataAPI.Handlers
{
    internal class UpdateFileCommandHandler(
        ApplicationDbContext context,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<UpdateFileCommand, FileDTO>
    {
        public async Task<FileDTO> Handle(UpdateFileCommand request, CancellationToken cancellationToken)
        {
            var userId = int.Parse(httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new Exception("User ID not found."));

            var file = await context.Files
                .Include(f => f.FileShares)
                .FirstOrDefaultAsync(f => f.Id == request.Id && f.OwnerId == userId, cancellationToken)
                ?? throw new Exception("File not found or authorization error.");

            mapper.Map(request, file);
            file.FileShares.Clear();
            file.FileShares.AddRange(mapper.Map<List<FileShare>>(request.FileShares));

            await context.SaveChangesAsync(cancellationToken);

            return mapper.Map<FileDTO>(file);
        }
    }
}