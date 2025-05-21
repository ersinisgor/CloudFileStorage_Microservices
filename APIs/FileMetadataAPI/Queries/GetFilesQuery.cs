using MediatR;
using FileMetadataAPI.DTOs;

namespace FileMetadataAPI.Queries
{
    internal class GetFilesQuery : IRequest<List<FileDTO>>
    {
    }
}