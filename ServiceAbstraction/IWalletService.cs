using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Enum;
using Domain.Entities.Struct;
using ServiceAbstraction.DTOs.WalletsDtos;

namespace ServiceAbstraction
{
    public interface IWalletService
    {
        Task<WalletDTO> CreateWalletAsync(CreateWalletDTO createWalletDTO);
        Task<IEnumerable<WalletDTO>> GetAllWalletsAsync(int userId);
        Task<Money> GetBalanceAsync(int WalletId);
        Task TransactionBetweenWalletAsync(int fromWalletId, int toWalletId, Money amount, MoneySource moneySource);
        Task DepositAsync(int walletId, Money amount, MoneySource moneySource);
        Task WithdrawAsync(int walletId, Money amount, MoneySource moneySource);
    }
}
