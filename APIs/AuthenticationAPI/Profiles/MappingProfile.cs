using AutoMapper;
using AuthenticationAPI.Models;
using AuthenticationAPI.DTOs;
using AuthenticationAPI.Commands;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AuthenticationAPI.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDTO>();
            CreateMap<RegisterCommand, User>();
        }
    }
}