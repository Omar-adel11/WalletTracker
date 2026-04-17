using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Enum;
using Domain.Entities.Struct;

namespace ServiceAbstraction.DTOs.TransactionDtos
{
    public class CreateTransactionDTO
    {
        public Money Amount { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
        public int WalletId { get; set; }
        public int CategoryId { get; set; }
        public TransactionType Type { get; set; }
    }
}
