using MediatR;

namespace FileMetadataAPI.Commands
{
    internal class DeleteFileCommand : IRequest
    {
        public int Id { get; set; }
    }
}