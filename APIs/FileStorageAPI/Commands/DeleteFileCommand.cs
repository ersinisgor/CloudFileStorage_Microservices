using MediatR;

namespace FileStorageAPI.Commands
{
    public class DeleteFileCommand : IRequest
    {
        public string FilePath { get; set; }
    }
}
