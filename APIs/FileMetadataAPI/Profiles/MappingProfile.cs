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
                .ForMember(dest => dest.IsOwner, opt => opt.Ignore());
            CreateMap<FileDTO, File>()
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => Enum.Parse<Visibility>(src.Visibility, true)));
            CreateMap<CreateFileCommand, File>()
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => Enum.Parse<Visibility>(src.Visibility.Trim(), true)))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
            CreateMap<UpdateFileCommand, File>()
                .ForMember(dest => dest.Visibility, opt => opt.MapFrom(src => Enum.Parse<Visibility>(src.Visibility, true)));
            CreateMap<FileShare, FileShareDTO>();
            CreateMap<FileShareDTO, FileShare>();
            CreateMap<UpdateFileDTO, UpdateFileCommand>()
                .ForMember(dest => dest.FileShares, opt => opt.Ignore()) 
                .AfterMap((src, dest) =>
                {
                    dest.FileShares = string.IsNullOrEmpty(src.SharedUserIds)
                        ? new List<FileShareDTO>()
                        : src.SharedUserIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s =>
                            {
                                if (int.TryParse(s.Trim(), out var userId) && userId > 0)
                                {
                                    return new FileShareDTO
                                    {
                                        UserId = userId,
                                        Permission = src.Permission
                                    };
                                }
                                return null;
                            })
                            .Where(fs => fs != null)
                            .ToList();
                });
        }
    }
}