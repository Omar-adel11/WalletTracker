using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Entities.Struct;
using ServiceAbstraction.DTOs.TransactionDtos;
using Shared;

namespace Service.Mapping.Transaction
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<Domain.Entities.Transaction,TransactionDTO>().ForMember(d => d.Category, s => s.MapFrom(src => src.Category.Name)); ;
            CreateMap<CreateTransactionDTO, Domain.Entities.Transaction>().ReverseMap()
                     .ForMember(dest => dest.source,opt => opt.Ignore());
            CreateMap<UpdateTransactionDTO, Domain.Entities.Transaction>()
                     .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Money?, Money>()
        .ConvertUsing((src, dest) => src ?? dest);

            CreateMap<PagedResult<Domain.Entities.Transaction>, PagedResult<TransactionDTO>>();

        }
    }
}
