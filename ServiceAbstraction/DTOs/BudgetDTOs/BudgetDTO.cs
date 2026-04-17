using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Struct;

namespace ServiceAbstraction.DTOs.BudgetDTOs
{
    public class BudgetDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsDisabled { get; set; } = false;
        public Money Limit { get; set; }
        public Money Spent { get; set; }



    }
}
