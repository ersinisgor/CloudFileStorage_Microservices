using FileMetadataAPI.DTOs;
using MediatR;

namespace FileMetadataAPI.Commands
{
    public class ShareFileCommand : IRequest<Unit>
    {
        public int FileId { get; set; }
        public string Visibility { get; set; }
        public List<FileShareDTO> FileShares { get; set; } = new();
    }
}
