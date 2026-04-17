using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Entities;
using ServiceAbstraction.DTOs.InstallmentsDTOs;

namespace Service.Mapping.Installments
{
    public class InstallmentsProfile : Profile
    {
        public InstallmentsProfile()
        {
            CreateMap<Domain.Entities.Installments, InstallmentDTO>().ForMember(d=>d.Category,s=>s.MapFrom(i=>i.Category.Name));
             CreateMap<Domain.Entities.Installments, CreateInstallmentDTO>().ReverseMap();
            CreateMap<Domain.Entities.Installments, UpdateInstallmentDTO>().ReverseMap()
                     .ForAllMembers(opts => opts.Condition((src,dest,srcMember) => srcMember != null));
        }
    }
}
