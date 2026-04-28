using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Struct;
using ServiceAbstraction.DTOs.InstallmentsDTOs;
using Shared;

namespace Service.Mapping.Installments
{
    public class InstallmentsProfile : Profile
    {
        public InstallmentsProfile()
        {
            CreateMap<Domain.Entities.Installments, InstallmentDTO>().ForMember(d=>d.Category,s=>s.MapFrom(i=>i.Category.Name));
            CreateMap<PagedResult<Domain.Entities.Installments>, PagedResult<InstallmentDTO>>();
            CreateMap<CreateInstallmentDTO, Domain.Entities.Installments>()
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => new Money
                {
                    Amount = src.Amount,
                    Currency = "EGP" 
                }))
                
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.transactions, opt => opt.Ignore());

            CreateMap<UpdateInstallmentDTO, Domain.Entities.Installments>()
    .ForAllMembers(opts =>
        opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
