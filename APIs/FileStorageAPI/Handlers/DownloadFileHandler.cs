using MediatR;
using FileStorageAPI.Queries.DownloadFile;
using FileStorageAPI.Models;
using System.Net.Mime;
using Microsoft.Extensions.Configuration;

namespace FileStorageAPI.Handlers
{
    public class DownloadFileHandler : IRequestHandler<DownloadFileQuery, FileStorage>
    {
        private readonly IConfiguration _configuration;
        //private readonly IHttpClientFactory _httpClientFactory;

        public DownloadFileHandler(IConfiguration configuration)
        {
            _configuration = configuration;
            //_httpClientFactory = httpClientFactory;
        }

        
        public async Task<FileStorage> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
        {
            if (!File.Exists(request.FilePath))
            {
                throw new FileNotFoundException("File not found.");
            }

            if (!request.FilePath.StartsWith(Path.Combine(Directory.GetCurrentDirectory(), _configuration.GetSection("FileStorage:StoragePath").Value)))
            {
                throw new UnauthorizedAccessException("Access to file is restricted.");
            }

            // FileMetadataAPI permission check with http => MOVE IT GATEWAY_API LATER
            //var client = _httpClientFactory.CreateClient("FileMetadataAPI");
            //var response = await client.GetAsync($"/api/files/{Path.GetFileName(request.FilePath)}/permissions");
            //if (!response.IsSuccessStatusCode)
            //{
            //    throw new UnauthorizedAccessException("You do not have permission to access this file.");
            //}

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