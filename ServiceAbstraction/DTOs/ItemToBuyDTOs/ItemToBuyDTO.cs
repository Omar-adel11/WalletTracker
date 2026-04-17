using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Struct;

namespace ServiceAbstraction.DTOs.ItemToBuyDTOs
{
    public class ItemToBuyDTO
    {
        public string Name { get; set; } = string.Empty;
        public Money Price { get; set; }
        public Money Amount { get; set; }
        public bool IsAchieved { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
