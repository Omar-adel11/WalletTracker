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
        Task<IEnumerable<TransactionDTO>> GetTransactionByWalletAsync(int userId,int walletId,int PageNumber,int PageSize);
        Task<TransactionDTO> GetTransactionByIdAsync(int userId,int Id);
        Task<TransactionDTO> CreateTransactionAsync(int userId, CreateTransactionDTO transactionDTO);
        Task DeleteTransactionAsync(int userId, int transactionId);
        Task UpdateTransactionAsync(int userId, UpdateTransactionDTO updateTransactionDTO);
    }
}
