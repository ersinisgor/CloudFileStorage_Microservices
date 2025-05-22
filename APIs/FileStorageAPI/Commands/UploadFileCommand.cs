using MediatR;

namespace FileStorageAPI.Commands.UploadFile
{
    public class UploadFileCommand : IRequest<string>
    {
        public IFormFile File { get; set; }
    }
}