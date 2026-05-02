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
using Domain.Exceptions.AuthExceptions;
using Domain.Exceptions.BadRequestException;
using Domain.Exceptions.MoneyInvalidOperationException;
using Domain.Exceptions.NullReferenceException;
using Service.Helper;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.Auth;
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

        public async Task<WalletDTO> GetWalletByIdAsync(int userId, int walletId)
        {
            var wallet = await GetAndAuthorizeWalletAsync(userId, walletId);
            return _mapper.Map<WalletDTO>(wallet);
        }
        public async Task<Money> GetBalanceAsync(int userId,int WalletId)
        {
            var wallet = await GetAndAuthorizeWalletAsync(userId, WalletId);
            var Money = new Money
            {
                Amount = wallet.TotalBalance,
                Currency = wallet.Currency
            };
            return Money;
        }

        public async Task<WalletDTO> CreateWalletAsync(CreateWalletDTO createWalletDTO)
        {
            var user = await _unitOfWork.Repository<User>()
                               .GetFirstOrDefaultAsync(u => u.Id == createWalletDTO.UserId);

            if (user is null) throw new UserNotFoundNullException();

            if (!user.IsPremium)
            {
                var walletCount = await _repo.CountAsync(w => w.UserId == createWalletDTO.UserId);
                if (walletCount >= PlanLimits.FreeWalletLimit)
                    throw new LimitExceededException(
                        $"wallet");
            }
            var wallet = _mapper.Map<Wallet>(createWalletDTO);
            await _repo.AddAsync(wallet);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<WalletDTO>(wallet);
        }

        public async Task DepositAsync(int userId,int walletId, Money amount, MoneySource moneySource)
        {
            var wallet = await GetAndAuthorizeWalletAsync(userId, walletId);
            wallet.ApplyTransaction(TransactionType.Income, moneySource, amount.Amount);
            await _unitOfWork.CompleteAsync();
        }

        public async Task WithdrawAsync(int userId,int walletId, Money amount, MoneySource moneySource)
        {
            var wallet = await GetAndAuthorizeWalletAsync(userId, walletId);
            wallet.ApplyTransaction(TransactionType.Expense, moneySource, amount.Amount);
            await _unitOfWork.CompleteAsync();
        }
        
        public async Task TransactionBetweenWalletAsync(int userId,int fromWalletId,int toWalletId, string ToUserName, Money amount, MoneySource moneySource)
        {
            var fromWallet = await GetAndAuthorizeWalletAsync(userId, fromWalletId);

            var userList = await _unitOfWork.Repository<User>().GetAsync(u => u.UserName == ToUserName);
            var user = userList.FirstOrDefault();
            if (user is null) throw new EntityNotFoundException("User");

            var toWallet = await GetAndAuthorizeWalletAsync(user.Id, toWalletId);
            if (fromWallet.Currency != toWallet.Currency) throw new CurrencyMismatchException();
           
            using var trans = await _unitOfWork.BeginTransactionAsync();
            try
            {
                fromWallet.ApplyTransaction(TransactionType.Expense, moneySource, amount.Amount);
                toWallet.ApplyTransaction(TransactionType.Income, moneySource, amount.Amount);
                await _unitOfWork.CompleteAsync();
                await trans.CommitAsync();
            }catch(Exception ex)
            {
                await trans.RollbackAsync();
                throw;
            }
        }

       private async Task<Wallet> GetAndAuthorizeWalletAsync(int userId,int walletId)
        {
            var wallet = await _repo.GetByIdAsync(walletId, w => w.Transactions!);
            if (wallet is null) throw new EntityNotFoundException("Wallet");
            if (wallet.UserId != userId) throw new UnAuthorizedException("Can't make transaction on this wallet");
            return wallet;
        }

       
    }
}
