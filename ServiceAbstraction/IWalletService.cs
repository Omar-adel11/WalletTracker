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
        Task<WalletDTO> GetWalletByIdAsync(int userId, int walletId);
        Task<Money> GetBalanceAsync(int userId, int WalletId);
        Task TransactionBetweenWalletAsync(int userId, int fromWalletId, int toWalletId,string ToUserName, Money amount, MoneySource moneySource);
        Task DepositAsync(int userId,int walletId, Money amount, MoneySource moneySource);
        Task WithdrawAsync(int userId,int walletId, Money amount, MoneySource moneySource);
    }
}
