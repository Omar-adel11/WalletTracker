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
using Domain.Exceptions.AuthExceptions;
using Domain.Exceptions.MoneyInvalidOperationException;
using Domain.Exceptions.NullReferenceException;
using Microsoft.AspNetCore.Mvc;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.TransactionDtos;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Service
{
    public class TransactionService(IUnitOfWork unitOfWork, IMapper _mapper) : ITransactionService
    {
        private IGenericRepository<Domain.Entities.Transaction> _repo = unitOfWork.Repository<Domain.Entities.Transaction>();
        public async Task<TransactionDTO> GetTransactionByIdAsync(int userId, int Id)
        {
            var transaction = await _repo.GetByIdAsync(Id, t => t.Wallet!, t => t.Category!);
            if (transaction is null) throw new EntityNotFoundException("transaction");
            if (transaction.UserId != userId) throw new UnAuthorizedException("Not authorized");
            return _mapper.Map<TransactionDTO>(transaction);

        }

        public async Task<IEnumerable<TransactionDTO>> GetTransactionByWalletAsync(int userId,int walletId, int PageSize,int PageNumber)
        {
            var wallet = await unitOfWork.Repository<Wallet>().GetByIdAsync(walletId);

            if (wallet == null)
                throw new EntityNotFoundException("Wallet");

            if (wallet.UserId != userId)
                throw new UnAuthorizedException("You are not authorized to view transactions for this wallet");

            var transactions = await _repo.GetAsyncFilteredWithPaginate(
    t => t.WalletId == walletId,
    t => t.CreatedAt,
    PageNumber,
    PageSize,
    t => t.Wallet!,
    t => t.Category!
);

            if (!transactions.Any()) throw new EntityNotFoundException("Transaction");

            return _mapper.Map<IEnumerable<TransactionDTO>>(transactions);

        }

        public async Task<TransactionDTO> CreateTransactionAsync(int userId,CreateTransactionDTO transactionDTO)
        {
            var wallet = await unitOfWork.Repository<Wallet>().GetByIdAsync(transactionDTO.WalletId);
            if (wallet is null) throw new EntityNotFoundException("wallet");
            if (wallet.UserId != userId) throw new UnAuthorizedException("Not authorized");
            transactionDTO.UserId = userId;
            using var tran = await unitOfWork.BeginTransactionAsync();
            try
            {
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
                transaction.Category = await unitOfWork.Repository<Category>().GetByIdAsync(transaction.CategoryId!.Value);
                await _repo.AddAsync(transaction);
                await unitOfWork.CompleteAsync();
                await tran.CommitAsync();
                return _mapper.Map<TransactionDTO>(transaction);
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                throw ex;
            }
         
        }

        public async Task DeleteTransactionAsync(int userId,int transactionId)
        {
            var transaction = await _repo.GetByIdAsync(transactionId);
            if (transaction is null) throw new EntityNotFoundException("transaction");
            if (transaction.UserId != userId) throw new UnAuthorizedException("Not authorized");
            _repo.Delete(transaction);
            await unitOfWork.CompleteAsync();
        }


        public async Task UpdateTransactionAsync(int userId, UpdateTransactionDTO dto)
        {
            
            var transaction = await _repo.GetByIdAsync(dto.Id, t => t.Wallet!);
            if (transaction is null) throw new EntityNotFoundException("transaction");
            if (transaction.UserId != userId) throw new UnAuthorizedException("Not authorized");

            var wallet = transaction.Wallet;
            var budgetRepo = unitOfWork.Repository<Budget>();
            using var tran = await unitOfWork.BeginTransactionAsync();
            try
            {
                bool isFinancialUpdate = dto.Amount != null ||
    (dto.Type.HasValue && dto.Type != transaction.Type) ||
    (dto.MoneySource.HasValue && dto.MoneySource != transaction.MoneySource) ||
    (dto.CategoryId.HasValue && dto.CategoryId != transaction.CategoryId);
                if (isFinancialUpdate)
                {

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

                }

                //map manually
                if (dto.Description != null) transaction.Description = dto.Description;
                if (dto.Date.HasValue) transaction.Date = dto.Date.Value;

                _repo.Update(transaction);
                await unitOfWork.CompleteAsync();
                await tran.CommitAsync();

            }catch(Exception ex)
            {
                await tran.RollbackAsync();
                throw ex;
            }

           
        }
        
    }
}
