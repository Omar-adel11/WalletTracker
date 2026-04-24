using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ServiceAbstraction.DTOs.ItemToBuyDTOs;

namespace Service.Mapping.ItemToBuy
{
    public class ItemToBuyProfile :Profile
    {
        public ItemToBuyProfile()
        {
            CreateMap<Domain.Entities.ItemToBuy, ItemToBuyDTO>();
                
             CreateMap<Domain.Entities.ItemToBuy, CreateItemToBuyDTO>().ReverseMap();
             CreateMap<Domain.Entities.ItemToBuy, UpdateItemToBuyDTO>();
        }
    }
}
