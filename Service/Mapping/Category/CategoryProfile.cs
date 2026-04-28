using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Entities;
using ServiceAbstraction.DTOs.CategoryDtos;
using ServiceAbstraction.DTOs.TransactionDtos;
using Shared;

namespace Service.Mapping.Category
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Domain.Entities.Category, CategoryDto>().ReverseMap();
            CreateMap<PagedResult<Domain.Entities.Category>, PagedResult<CategoryDto>>();
        }
    }
}
