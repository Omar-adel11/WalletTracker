using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Entities.Struct;
using Domain.Exceptions.MoneyInvalidOperationException;
using Domain.Exceptions.NullReferenceException;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.WalletsDtos;

namespace Service
{
    public class WalletService(IUnitOfWork _unitOfWork, IMapper _mapper) : IWalletService
    {
        private IGenericRepository<Wallet> _repo = _unitOfWork.Repository<Wallet>();

        public async Task<IEnumerable<WalletDTO>> GetAllWalletsAsync(int userId)
        {
            var wallets = await _repo.GetAsync(w => w.UserId == userId);
            if(!wallets.Any()) throw new EntityNotFoundException("Wallet");
            var walletDTOs = _mapper.Map<IEnumerable<WalletDTO>>(wallets);
            return walletDTOs;
        }

        public async Task<Money> GetBalanceAsync(int WalletId)
        {
            var wallet = await _repo.GetByIdAsync(WalletId);
            if(wallet is null) throw new EntityNotFoundException("Wallet");
            var Money = new Money
            {
                Amount = wallet.TotalBalance,
                Currency = wallet.Currency
            };
            return Money;
        }

        public async Task<WalletDTO> CreateWalletAsync(CreateWalletDTO createWalletDTO)
        {
            var wallet = _mapper.Map<Wallet>(createWalletDTO);
            await _repo.AddAsync(wallet);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<WalletDTO>(wallet);
        }

        public async Task DepositAsync(int walletId, Money amount, MoneySource moneySource)
        {
            var wallet = await _repo.GetByIdAsync(walletId,w=>w.Transactions!);
            if (wallet is null) throw new EntityNotFoundException("Wallet");
            if (wallet.Currency != amount.Currency) throw new CurrencyMismatchException();
            var IsCash = moneySource == MoneySource.Cash ;
            var IsCredit = moneySource == MoneySource.Credit;
            if(!IsCash && !IsCredit) throw new InvalidSourceException(moneySource.ToString());
            if (IsCash)
            {
                wallet.Cash += amount.Amount;
            }
            else
            {
                wallet.Credit += amount.Amount;
            }
            var transaction = new Transaction
            {
                WalletId = wallet.id,
                UserId = wallet.UserId,
                Amount = amount,
                Description = $"Deposit of {amount.Amount} {amount.Currency} to wallet {wallet.id}",
                Type = TransactionType.Income,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _unitOfWork.Repository<Transaction>().AddAsync(transaction);

            await _unitOfWork.CompleteAsync();


        }

        public async Task WithdrawAsync(int walletId, Money amount, MoneySource moneySource)
        {
            var wallet = await _repo.GetByIdAsync(walletId, w => w.Transactions!);
            if (wallet is null) throw new EntityNotFoundException("Wallet");
            if (wallet.Currency != amount.Currency) throw new CurrencyMismatchException();

            var IsCash = moneySource == MoneySource.Cash;
            var IsCredit = moneySource == MoneySource.Credit;
            if (!IsCash && !IsCredit) throw new InvalidSourceException(moneySource.ToString());
            if (IsCash)
            {
                if(wallet.Cash < amount.Amount) throw new NotEnoughBalanceException();
                wallet.Cash -= amount.Amount;
            }
            else
            {
                if (wallet.Credit < amount.Amount) throw new NotEnoughBalanceException();
                wallet.Credit -= amount.Amount;
            }
            var transaction = new Transaction
            {
                WalletId = wallet.id,
                UserId = wallet.UserId,
                Amount = amount,
                Description = $"Withdraw of {amount.Amount} {amount.Currency} from wallet {wallet.id}",
                Type = TransactionType.Expense,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _unitOfWork.Repository<Transaction>().AddAsync(transaction);

            await _unitOfWork.CompleteAsync();
        }
        
        public async Task TransactionBetweenWalletAsync(int fromWalletId, int toWalletId, Money amount, MoneySource moneySource)
        {
            var fromWallet = await _repo.GetByIdAsync(fromWalletId, w => w.Transactions!);
            if (fromWallet is null) throw new EntityNotFoundException("Wallet");
            var toWallet = await _repo.GetByIdAsync(toWalletId, w => w.Transactions!);
            if (toWallet is null) throw new EntityNotFoundException("Wallet");

            if(fromWallet.Currency != toWallet.Currency) throw new CurrencyMismatchException();
            var IsCash = moneySource == MoneySource.Cash;
            var IsCredit = moneySource == MoneySource.Credit;
            if (!IsCash && !IsCredit) throw new InvalidSourceException(moneySource.ToString());
            if (IsCash)
            {
                if(fromWallet.Cash < amount.Amount) throw new NotEnoughBalanceException();
                fromWallet.Cash -= amount.Amount;
                toWallet.Cash += amount.Amount;
            }
            else
            {
                if (fromWallet.Credit < amount.Amount) throw new NotEnoughBalanceException();
                fromWallet.Credit -= amount.Amount;
                toWallet.Credit += amount.Amount;
            }
            var transaction = new Transaction
            {
                WalletId = fromWallet.id,
                UserId = fromWallet.UserId,
                Amount = amount,
                Description = $"Withdraw of {amount.Amount} {amount.Currency} from wallet {fromWallet.id}",
                Type = TransactionType.Expense,
                CreatedAt = DateTimeOffset.UtcNow
            };
            var transaction2 = new Transaction
            {
                WalletId = toWallet.id,
                UserId = toWallet.UserId,
                Amount = amount,
                Description = $"Deposit of {amount.Amount} {amount.Currency} to wallet {toWallet.id}",
                Type = TransactionType.Income,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _unitOfWork.Repository<Transaction>().AddAsync(transaction);
            await _unitOfWork.Repository<Transaction>().AddAsync(transaction2);

            await _unitOfWork.CompleteAsync();

        }

       
    }
}
