using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Struct;
using ServiceAbstraction.DTOs.WalletsDtos;

namespace ServiceAbstraction
{
    public interface IWalletService
    {
        Task<IEnumerable<WalletDTO>> GetAllWallets(int userId);
        Task<Money> GetBalanceAsync(int userId,int WalletId);
        Task<bool> TransactionBetweenWalletAsync(int fromWalletId, int toWalletId, decimal amount);
        Task<int> UpdateBalanceAsync(int userId, Money newBalance);
    }
}
