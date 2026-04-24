using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Enum;

namespace ServiceAbstraction.DTOs.InstallmentsDTOs
{
    public class PayInstallmentDTO
    {
        public int WalletId { get; set; }
        public MoneySource source { get; set; }
    }
}
