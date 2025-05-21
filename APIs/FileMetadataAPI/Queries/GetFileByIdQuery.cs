using MediatR;
using FileMetadataAPI.DTOs;

namespace FileMetadataAPI.Queries
{
    internal class GetFileByIdQuery : IRequest<FileDTO>
    {
        public int Id { get; set; }
    }
}