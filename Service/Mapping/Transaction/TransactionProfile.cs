using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ServiceAbstraction.DTOs.TransactionDtos;

namespace Service.Mapping.Transaction
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<Domain.Entities.Transaction,TransactionDTO>().ForMember(d => d.Category, s => s.MapFrom(src => src.Category.Name)); ;
            CreateMap<CreateTransactionDTO, Domain.Entities.Transaction>().ReverseMap()
                     .ForMember(dest => dest.source,opt => opt.Ignore());
            CreateMap<UpdateTransactionDTO, Domain.Entities.Transaction>().ReverseMap()
                     .ForAllMembers(opts => opts.Condition((src, DestinationMemberNamingConvention, srcMember) => srcMember != null));
                     


        }
    }
}
