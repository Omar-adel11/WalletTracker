using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.DTOs.TransactionDtos
{
    public class UpdateTransactionDTO
    {
        public int id { get; set; }
        public int WalletId { get; set; }
        public decimal? amount { get; set; }
        public string? description { get; set; }
        public DateTimeOffset? date { get; set; } = DateTimeOffset.UtcNow;
        int? categoryId { get; set; }
    }
}
