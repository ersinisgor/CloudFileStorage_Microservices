using MediatR;
using FileStorageAPI.Models;

namespace FileStorageAPI.Queries.DownloadFile
{
    public class DownloadFileQuery : IRequest<FileStorage>
    {
        public string FilePath { get; set; }
    }
}