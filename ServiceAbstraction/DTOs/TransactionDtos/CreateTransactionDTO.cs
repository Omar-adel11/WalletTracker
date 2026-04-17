using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.DTOs.TransactionDtos
{
    public class TransactionDTO
    {
        public decimal amount { get; set; }
        public string? description { get; set; }
        public DateTimeOffset date { get; set; } = DateTimeOffset.UtcNow;
        public int userId { get; set; }
        public int wallet_id { get; set; }
        int categoryId { get; set; }
    }
}
