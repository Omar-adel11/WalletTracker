using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Entities;
using ServiceAbstraction.DTOs.Auth;

namespace Service.Mapping.Auth
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.Token, opt => opt.Ignore()).ReverseMap();
            CreateMap<UserSignUpDTO, User>();
            
        }
    }
}
