using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using ServiceAbstraction.DTOs.TransactionDtos;
using Shared;

namespace ServiceAbstraction
{
    public interface ITransactionService
    {
        Task<PagedResult<TransactionDTO>> GetTransactionByWalletAsync(int userId,int walletId, int? PageNumber = 1, int? PageSize = 5);
        Task<TransactionDTO> GetTransactionByIdAsync(int userId,int Id);
        Task<TransactionDTO> CreateTransactionAsync(int userId, CreateTransactionDTO transactionDTO);
        Task DeleteTransactionAsync(int userId, int transactionId);
        Task UpdateTransactionAsync(int userId, UpdateTransactionDTO updateTransactionDTO);
    }
}
