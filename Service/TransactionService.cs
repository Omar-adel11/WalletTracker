using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Exceptions.MoneyInvalidOperationException;
using Domain.Exceptions.NullReferenceException;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.TransactionDtos;

namespace Service
{
    public class TransactionService(IUnitOfWork unitOfWork, IMapper _mapper) : ITransactionService
    {
        private IGenericRepository<Domain.Entities.Transaction> _repo = unitOfWork.Repository<Domain.Entities.Transaction>();
        public async Task<TransactionDTO> GetTransactionByIdAsync(int Id)
        {
            var transaction = await _repo.GetByIdAsync(Id, t => t.Wallet!, t => t.Category!);
            if (transaction is null) throw new EntityNotFoundException("transaction");
            return _mapper.Map<TransactionDTO>(transaction);

        }

        public async Task<IEnumerable<TransactionDTO>> GetTransactionByWalletAsync(int walletId, int pageNumber, int pageSize)
        {
            var transactions = await _repo.GetAsyncFilteredWithPaginate(
        t => t.WalletId == walletId,
        pageNumber,
        pageSize,
        t => t.Wallet!,
        t => t.Category!);

            if (!transactions.Any()) throw new EntityNotFoundException("Transaction");

            return _mapper.Map<IEnumerable<TransactionDTO>>(transactions);

        }

        public async Task<TransactionDTO> CreateTransactionAsync(CreateTransactionDTO transactionDTO)
        {
            var wallet = await unitOfWork.Repository<Wallet>().GetByIdAsync(transactionDTO.WalletId);
            if (wallet is null) throw new EntityNotFoundException("wallet");

            // 1. Update the correct balance based on Source
            if (transactionDTO.Type == TransactionType.Income)
            {
                if (transactionDTO.MoneySource == MoneySource.Cash) wallet.Cash += transactionDTO.Amount.Amount;
                else wallet.Credit += transactionDTO.Amount.Amount;
            }
            else // Expense
            {
                if (transactionDTO.MoneySource == MoneySource.Cash)
                {
                    if (wallet.Cash < transactionDTO.Amount.Amount) throw new NotEnoughBalanceException();
                    wallet.Cash -= transactionDTO.Amount.Amount;
                }
                else
                {
                    if (wallet.Credit < transactionDTO.Amount.Amount) throw new NotEnoughBalanceException();
                    wallet.Credit -= transactionDTO.Amount.Amount;
                }
            }

            if (transactionDTO.Type == TransactionType.Expense)
            {
                // Look for an active budget for this user and category
                var budgetRepo = unitOfWork.Repository<Budget>();
                var budgets = await budgetRepo.GetAsync(b => b.UserId == wallet.UserId &&
                                                             b.CategoryId == transactionDTO.CategoryId);

                var activeBudget = budgets.FirstOrDefault();
                if (activeBudget != null)
                {
                    // Sync the budget "Spent" amount
                    activeBudget.Spent = activeBudget.Spent with
                    {
                        Amount = activeBudget.Spent.Amount + transactionDTO.Amount.Amount
                    };
                    budgetRepo.Update(activeBudget);
                }
            }

            // 2. Map and Save
            var transaction = _mapper.Map<Domain.Entities.Transaction>(transactionDTO);
            transaction.CreatedAt = DateTimeOffset.UtcNow;

            await _repo.AddAsync(transaction);
            await unitOfWork.CompleteAsync();

            return _mapper.Map<TransactionDTO>(transaction);
        }

        public async Task DeleteTransactionAsync(int transactionId)
        {
            var transaction = await _repo.GetByIdAsync(transactionId);
            if (transaction is null) throw new EntityNotFoundException("transaction");
            _repo.Delete(transaction);
            await unitOfWork.CompleteAsync();
        }


        public async Task UpdateTransactionAsync(UpdateTransactionDTO dto)
        {
            var transaction = await _repo.GetByIdAsync(dto.Id, t => t.Wallet!);
            if (transaction is null) throw new EntityNotFoundException("transaction");

            var wallet = transaction.Wallet;
            var budgetRepo = unitOfWork.Repository<Budget>();

            if (transaction.Type == TransactionType.Income)
            {
                if (transaction.MoneySource == MoneySource.Cash) wallet.Cash -= transaction.Amount.Amount;
                else wallet.Credit -= transaction.Amount.Amount;
            }
            else 
            {
                if (transaction.MoneySource == MoneySource.Cash) wallet.Cash += transaction.Amount.Amount;
                else wallet.Credit += transaction.Amount.Amount;

                var oldBudgets = await budgetRepo.GetAsync(b => b.UserId == wallet.UserId && b.CategoryId == transaction.CategoryId);
                var oldBudget = oldBudgets.FirstOrDefault();
                if (oldBudget != null)
                {
                    oldBudget.Spent = oldBudget.Spent with { Amount = oldBudget.Spent.Amount - transaction.Amount.Amount };
                    budgetRepo.Update(oldBudget);
                }
            }

            decimal newAmount = dto.Amount.Value.Amount;

            if (dto.Type == TransactionType.Income)
            {
                if (dto.MoneySource == MoneySource.Cash) wallet.Cash += newAmount;
                else wallet.Credit += newAmount;
            }
            else 
            {
                if (dto.MoneySource == MoneySource.Cash)
                {
                    if (wallet.Cash < newAmount) throw new NotEnoughBalanceException();
                    wallet.Cash -= newAmount;
                }
                else
                {
                    if (wallet.Credit < newAmount) throw new NotEnoughBalanceException();
                    wallet.Credit -= newAmount;
                }

                
                var newBudgets = await budgetRepo.GetAsync(b => b.UserId == wallet.UserId && b.CategoryId == dto.CategoryId);
                var newBudget = newBudgets.FirstOrDefault();
                if (newBudget != null)
                {
                    newBudget.Spent = newBudget.Spent with { Amount = newBudget.Spent.Amount + newAmount };
                    budgetRepo.Update(newBudget);
                }
            }

            
            _mapper.Map(dto, transaction);
            transaction.UpdatedAt = DateTimeOffset.UtcNow;
            _repo.Update(transaction);
            await unitOfWork.CompleteAsync();
        }
        
    }
}
