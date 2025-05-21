using MediatR;
using FileMetadataAPI.DataContext;
using FileMetadataAPI.DTOs;
using AutoMapper;
using System.Security.Claims;
using FileMetadataAPI.Commands;
using File = FileMetadataAPI.Models.File;

namespace FileMetadataAPI.Handlers
{
    internal class CreateFileCommandHandler(
        ApplicationDbContext context,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<CreateFileCommand, FileDTO>
    {
        public async Task<FileDTO> Handle(CreateFileCommand request, CancellationToken cancellationToken)
        {
            var userId = int.Parse(httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new Exception("User ID not found."));

            var file = mapper.Map<File>(request);
            file.OwnerId = userId;
            file.UploadDate = DateTime.UtcNow;

            context.Files.Add(file);
            await context.SaveChangesAsync(cancellationToken);

            return mapper.Map<FileDTO>(file);
        }
    }
}