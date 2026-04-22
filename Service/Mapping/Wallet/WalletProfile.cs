using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Struct;
using ServiceAbstraction.DTOs.WalletsDtos;

namespace Service.Mapping.Wallet
{
    public class WalletProfile : Profile
    {
        public WalletProfile()
        {
           
            CreateMap<Domain.Entities.Wallet, WalletDTO>()
                .ForMember(dest => dest.Cash, opt => opt.MapFrom(src => new Money
                {
                    Amount = src.Cash,
                    Currency = src.Currency
                }))
                .ForMember(dest => dest.Credit, opt => opt.MapFrom(src => new Money
                {
                    Amount = src.Credit,
                    Currency = src.Currency
                }))
                .ForMember(dest => dest.Pended, opt => opt.MapFrom(src => new Money
                {
                    Amount = src.Pended,
                    Currency = src.Currency
                }));
            CreateMap<Domain.Entities.Wallet, CreateWalletDTO>().ReverseMap();
            
        }
    }
}
