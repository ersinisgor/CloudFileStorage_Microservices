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

            var file = await context.Files
                .Include(f => f.FileShares)
                .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException("File not found.");

            if (file.OwnerId != userId)
            {
                throw new ForbiddenException("Only the file owner can update this file.");
            }

            mapper.Map(request, file);
            file.FileShares.Clear();
            file.FileShares.AddRange(mapper.Map<List<FileShare>>(request.FileShares));

            await context.SaveChangesAsync(cancellationToken);
            var fileDto = mapper.Map<FileDTO>(file);
            fileDto.IsOwner = file.OwnerId == userId;
            return fileDto;
        }
    }
}