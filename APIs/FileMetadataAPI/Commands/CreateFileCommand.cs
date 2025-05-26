using MediatR;
using FileMetadataAPI.DTOs;
using FileMetadataAPI.Models;

namespace FileMetadataAPI.Commands
{
    public class CreateFileCommand : IRequest<FileDTO>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Visibility { get; set; }
        public IFormFile File { get; set; } // Added for file upload
        public List<FileShareDTO> FileShares { get; set; } = new();
    }
}