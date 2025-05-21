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
        }
    }
}