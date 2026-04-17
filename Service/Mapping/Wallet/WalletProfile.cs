using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Entities;
using ServiceAbstraction.DTOs.WalletsDtos;

namespace Service.Mapping.Wallet
{
    public class WalletProfile : Profile
    {
        public WalletProfile()
        {
            CreateMap<Domain.Entities.Wallet, WalletDTO>();
            
        }
    }
}
