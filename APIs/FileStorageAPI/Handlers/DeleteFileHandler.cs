using FileStorageAPI.Commands;
using MediatR;

namespace FileStorageAPI.Handlers
{
    public class DeleteFileHandler : IRequestHandler<DeleteFileCommand>
    {
        private readonly IConfiguration _configuration;

        public DeleteFileHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task Handle(DeleteFileCommand request, CancellationToken cancellationToken)
        {
            var storagePath = _configuration.GetSection("FileStorage:StoragePath").Value;
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), storagePath, request.FilePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
