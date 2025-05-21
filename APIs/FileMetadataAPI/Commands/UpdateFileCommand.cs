using MediatR;
using FileMetadataAPI.DTOs;

namespace FileMetadataAPI.Commands
{
    internal class UpdateFileCommand : IRequest<FileDTO>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Visibility { get; set; }
        public List<FileShareDTO> FileShares { get; set; } = new();
    }
}