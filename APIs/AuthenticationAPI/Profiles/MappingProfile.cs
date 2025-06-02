using AutoMapper;
using AuthenticationAPI.Models;
using AuthenticationAPI.DTOs;
using AuthenticationAPI.Commands;

namespace AuthenticationAPI.Profiles
{
    internal class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDTO>();
            CreateMap<RegisterCommand, User>();
            CreateMap<User, UserInfo>().ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
        }
    }
}