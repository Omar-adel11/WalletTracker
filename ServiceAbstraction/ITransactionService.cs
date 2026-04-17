using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using ServiceAbstraction.DTOs.TransactionDtos;

namespace ServiceAbstraction
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionDTO>> GetTransactionByWalletAsync(int walletId, int pageNumber, int pageSize);
        Task<TransactionDTO> GetTransactionByIdAsync(int Id);
        Task<TransactionDTO> CreateTransactionAsync(CreateTransactionDTO transactionDTO);
        Task DeleteTransactionAsync(int transactionId);
        Task UpdateTransactionAsync(UpdateTransactionDTO updateTransactionDTO);
    }
}
