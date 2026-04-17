using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.DTOs.InstallmentsDTOs
{
    public class CreateInstallmentDTO
    {
        public int userId { get; set; }
        public int walletId { get; set; }
        public decimal amount { get; set; }
        public string? description { get; set; }
        public int categoryId { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public int? NoOfPaidInstallments { get; set; }
        public string? To { get; set; }
    }
}
