using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Struct;

namespace ServiceAbstraction.DTOs.ItemToBuyDTOs
{
    public class CreateItemToBuyDTO
    {
        
        public int UserId { get; set; }
        public int categoryId { get; set; }
        public int walletId { get; set; }
        public string Name { get; set; } = string.Empty;
        public Money Price { get; set; }
        public Money Amount { get; set; }
    }
}
