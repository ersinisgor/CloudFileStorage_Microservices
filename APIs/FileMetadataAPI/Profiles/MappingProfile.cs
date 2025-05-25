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
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => src.Visibility.ToString()))
                .ForMember(dest => dest.Path, opt => opt.MapFrom(src => src.Path));
            CreateMap<FileDTO, File>()
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => Enum.Parse<Visibility>(src.Visibility)))
                .ForMember(dest => dest.Path, opt => opt.MapFrom(src => src.Path));
            CreateMap<CreateFileCommand, File>()
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => Enum.Parse<Visibility>(src.Visibility)));
            CreateMap<UpdateFileCommand, File>()
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => Enum.Parse<Visibility>(src.Visibility)));
            CreateMap<FileShare, FileShareDTO>();
            CreateMap<FileShareDTO, FileShare>();
        }
    }
}