using MediatR;
using FileStorageAPI.Queries.DownloadFile;
using FileStorageAPI.Models;
using System.Net.Mime;

namespace FileStorageAPI.Handlers
{
    public class DownloadFileHandler : IRequestHandler<DownloadFileQuery, FileStorage>
    {
        public async Task<FileStorage> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
        {
            if (!File.Exists(request.FilePath))
            {
                throw new FileNotFoundException("File not found.");
            }

            var content = await File.ReadAllBytesAsync(request.FilePath, cancellationToken);
            var contentType = MediaTypeNames.Application.Octet;
            var fileName = Path.GetFileName(request.FilePath);

            return new FileStorage
            {
                Content = content,
                ContentType = contentType,
                FileName = fileName
            };
        }
    }
}