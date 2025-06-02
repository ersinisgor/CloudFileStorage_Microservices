using FileStorageAPI.Commands;
using MediatR;

namespace FileStorageAPI.Handlers
{
    public class DeleteFileHandler(IConfiguration configuration) : IRequestHandler<DeleteFileCommand>
    {
        public async Task Handle(DeleteFileCommand request, CancellationToken cancellationToken)
        {
            var storagePath = configuration.GetSection("FileStorage:StoragePath").Value;
            if (string.IsNullOrEmpty(storagePath))
            {
                throw new InvalidOperationException("Storage path is not configured.");
            }

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), storagePath, request.FilePath);
            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath), cancellationToken);
            }
        }
    }
}