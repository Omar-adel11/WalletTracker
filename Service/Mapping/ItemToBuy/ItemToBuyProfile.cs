using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ServiceAbstraction.DTOs.ItemToBuyDTOs;
using Shared;

namespace Service.Mapping.ItemToBuy
{
    public class ItemToBuyProfile :Profile
    {
        public ItemToBuyProfile()
        {
            CreateMap<Domain.Entities.ItemToBuy, ItemToBuyDTO>();
            CreateMap<PagedResult<Domain.Entities.ItemToBuy>, PagedResult<ItemToBuyDTO>>();
                
             CreateMap<Domain.Entities.ItemToBuy, CreateItemToBuyDTO>().ReverseMap();
             CreateMap<Domain.Entities.ItemToBuy, UpdateItemToBuyDTO>();
        }
    }
}
