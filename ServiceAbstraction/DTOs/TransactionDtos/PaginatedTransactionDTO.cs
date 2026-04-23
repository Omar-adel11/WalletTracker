using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.DTOs.TransactionDtos
{
    public class PaginatedTransactionDTO
    {
        public int userId { get; set; }
        public int walletId { get; set; }
        public int? pageNumber { get; set; } = 1;
        public int? pageSize { get; set; } = 10;
    }
}
