using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ServiceAbstraction.DTOs.BudgetDTOs;

namespace Service.Mapping.Budget
{
    public class BudgetProfile : Profile
    {
        public BudgetProfile()
        {
            CreateMap<Domain.Entities.Budget, BudgetDTO>().ForMember(d=>d.Category,s=>s.MapFrom(b=>b.Category.Name));
            CreateMap<Domain.Entities.Budget, CreateBudgetDTO>().ReverseMap();
            CreateMap<Domain.Entities.Budget, UpdateBudgetDTO>().ReverseMap()
                     .ForAllMembers(opts => opts.Condition((src, DestinationMemberNamingConvention, srcMember) => srcMember != null)); 
             
        }
    }
}
