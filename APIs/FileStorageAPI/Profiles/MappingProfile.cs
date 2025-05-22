using AutoMapper;
using FileStorageAPI.Commands.UploadFile;
using FileStorageAPI.DTOs;

namespace FileStorageAPI.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UploadFileRequest, UploadFileCommand>();
        }
    }
}
