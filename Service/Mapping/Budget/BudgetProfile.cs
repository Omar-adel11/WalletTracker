using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Entities.Struct;
using ServiceAbstraction.DTOs.BudgetDTOs;

namespace Service.Mapping.Budget
{
    public class BudgetProfile : Profile
    {
        public BudgetProfile()
        {
            CreateMap<Domain.Entities.Budget, BudgetDTO>().ForMember(d=>d.Category,s=>s.MapFrom(b=>b.Name));
            CreateMap<CreateBudgetDTO, Domain.Entities.Budget>()
        .ForMember(dest => dest.Limit, opt => opt.MapFrom(src => new Money
        {
            Amount = src.Limit,
            Currency = src.Currency
        }))
        .ForMember(dest => dest.Spent, opt => opt.MapFrom(src => new Money
        {
            Amount = src.Amount,
            Currency = src.Currency
        }))
        // Ensure EF doesn't try to track these as new objects
        .ForMember(dest => dest.User, opt => opt.Ignore())
        .ForMember(dest => dest.Wallet, opt => opt.Ignore())
        .ForMember(dest => dest.Category, opt => opt.Ignore());
            CreateMap<Domain.Entities.Budget, UpdateBudgetDTO>().ReverseMap()
                     .ForAllMembers(opts => opts.Condition((src, DestinationMemberNamingConvention, srcMember) => srcMember != null)); 
             
        }
    }
}
