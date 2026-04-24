using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.DTOs.BudgetDTOs
{
    public class CreateBudgetDTO
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public decimal Limit { get; set; }
        public int CategoryId { get; set; }
        public int WalletId { get; set; }
    }
}
