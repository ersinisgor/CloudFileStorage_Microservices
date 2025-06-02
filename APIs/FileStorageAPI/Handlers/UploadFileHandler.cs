using MediatR;
using FileStorageAPI.Commands.UploadFile;

namespace FileStorageAPI.Handlers
{
    public class UploadFileHandler(IConfiguration configuration) : IRequestHandler<UploadFileCommand, string>
    {
        public async Task<string> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            var storagePath = configuration.GetSection("FileStorage:StoragePath").Value;
            if (string.IsNullOrEmpty(storagePath))
            {
                throw new InvalidOperationException("Storage path is not configured.");
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.File.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), storagePath, fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream, cancellationToken);
            }

            return filePath;
        }
    }
}