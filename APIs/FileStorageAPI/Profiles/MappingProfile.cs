using AutoMapper;
using FileStorageAPI.Commands.UploadFile;
using FileStorageAPI.DTOs;
using FileStorageAPI.Queries.DownloadFile;

namespace FileStorageAPI.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UploadFileRequest, UploadFileCommand>();
            CreateMap<DownloadFileRequest, DownloadFileQuery>();
        }
    }
}
