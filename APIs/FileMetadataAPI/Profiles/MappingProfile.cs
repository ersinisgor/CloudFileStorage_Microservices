using AutoMapper;
using FileMetadataAPI.Models;
using FileMetadataAPI.DTOs;
using FileMetadataAPI.Commands;
using File = FileMetadataAPI.Models.File;
using FileShare = FileMetadataAPI.Models.FileShare;

namespace FileMetadataAPI.Profiles
{
    internal class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<File, FileDTO>()
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility.ToString()));
            CreateMap<FileDTO, File>()
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => Enum.Parse<Visibility>(src.Visibility, true)));
            CreateMap<CreateFileCommand, File>()
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => Enum.Parse<Visibility>(src.Visibility.Trim(), true)));
            CreateMap<UpdateFileCommand, File>()
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => Enum.Parse<Visibility>(src.Visibility, true)));
            CreateMap<FileShare, FileShareDTO>();
            CreateMap<FileShareDTO, FileShare>();
        }
    }
}