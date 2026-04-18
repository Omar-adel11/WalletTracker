using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Struct;

namespace ServiceAbstraction.DTOs.ItemToBuyDTOs
{
    public class UpdateItemToBuyDTO
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string? Name { get; set; } 
        public Money? Price { get; set; }
        
    }
}
