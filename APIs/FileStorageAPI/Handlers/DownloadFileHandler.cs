using MediatR;
using FileStorageAPI.Queries.DownloadFile;
using FileStorageAPI.Models;
using System.Net.Mime;

namespace FileStorageAPI.Handlers
{
    public class DownloadFileHandler(IConfiguration configuration) : IRequestHandler<DownloadFileQuery, FileStorage>
    {
        public async Task<FileStorage> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
        {
            var storagePath = configuration.GetSection("FileStorage:StoragePath").Value;
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), storagePath, request.FilePath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("File not found.");
            }
            
            if (!fullPath.StartsWith(Path.Combine(Directory.GetCurrentDirectory(), storagePath)))
            {
                throw new UnauthorizedAccessException("Access to file is restricted.");
            }

            var content = await File.ReadAllBytesAsync(fullPath, cancellationToken);
            var contentType = MediaTypeNames.Application.Octet;
            var fileName = Path.GetFileName(fullPath);

            return new FileStorage
            {
                Content = content,
                ContentType = contentType,
                FileName = fileName
            };
        }
    }
}