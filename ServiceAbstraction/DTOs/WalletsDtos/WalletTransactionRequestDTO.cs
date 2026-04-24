using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Enum;
using Domain.Entities.Struct;

namespace ServiceAbstraction.DTOs.WalletsDtos
{
    public class WalletTransactionRequestDTO
    {
        public Money Amount { get; set; }
        public MoneySource Source { get; set; }
        // Optional: for transfers
        public int? ToWalletId { get; set; }
        public string? ToUserName { get; set; }
    }
}
