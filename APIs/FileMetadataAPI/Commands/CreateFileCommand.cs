using MediatR;
using FileMetadataAPI.DTOs;

namespace FileMetadataAPI.Commands
{
    public class CreateFileCommand : IRequest<FileDTO>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Visibility { get; set; }
        public List<FileShareDTO> FileShares { get; set; } = new();
    }
}