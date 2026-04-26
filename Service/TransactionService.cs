using System;
using System.Collections.Generic;
using System.Data.Common;
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
using Shared;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Service
{
    public class TransactionService(IUnitOfWork unitOfWork, IMapper _mapper) : ITransactionService
    {
        private IGenericRepository<Domain.Entities.Transaction> _repo = unitOfWork.Repository<Domain.Entities.Transaction>();
        public async Task<TransactionDTO> GetTransactionByIdAsync(int userId, int Id)
        {
            var transaction = await GetAndAuthorizeTransactionAsync(Id, userId);
            return _mapper.Map<TransactionDTO>(transaction);
        }

        public async Task<PagedResult<TransactionDTO>> GetTransactionByWalletAsync(int userId, int walletId, int PageSize, int PageNumber)
        {
            var wallet = await unitOfWork.Repository<Wallet>().GetByIdAsync(walletId);
            if (wallet == null) throw new EntityNotFoundException("Wallet");
            if (wallet.UserId != userId) throw new UnAuthorizedException("You are not authorized to view transactions for this wallet");

            var transactions = await _repo.GetAsyncFilteredWithPaginate(t => t.WalletId == walletId,
                                                                        t => t.CreatedAt,
                                                                        PageNumber,
                                                                        PageSize,
                                                                        t => t.Wallet!,
                                                                        t => t.Category!
                                                                        );
            if (!transactions.Items.Any()) throw new EntityNotFoundException("Transaction");
            return _mapper.Map< PagedResult<TransactionDTO>>(transactions);
        }

        public async Task<TransactionDTO> CreateTransactionAsync(int userId, CreateTransactionDTO transactionDTO)
        {
            var wallet = await unitOfWork.Repository<Wallet>().GetByIdAsync(transactionDTO.WalletId);
            if (wallet is null) throw new EntityNotFoundException("wallet");
            if (wallet.UserId != userId) throw new UnAuthorizedException("Not authorized");

            transactionDTO.UserId = userId;
            using var tran = await unitOfWork.BeginTransactionAsync();
            try
            {
                wallet.ApplyTransaction(transactionDTO.Type, transactionDTO.MoneySource, transactionDTO.Amount.Amount,transactionDTO.CategoryId);

                if (transactionDTO.Type == TransactionType.Expense)
                {
                    await SyncBudgetAsync(userId,transactionDTO.CategoryId,transactionDTO.Amount.Amount);
                }

                // 2. Map and Save
                var transaction = _mapper.Map<Domain.Entities.Transaction>(transactionDTO);
                transaction.CreatedAt = DateTimeOffset.UtcNow;
                transaction.Category = await unitOfWork.Repository<Category>().GetByIdAsync(transaction.CategoryId!.Value);
                await unitOfWork.CompleteAsync();
                await tran.CommitAsync();
                return _mapper.Map<TransactionDTO>(transaction);
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                throw;
            }

        }

        public async Task DeleteTransactionAsync(int userId, int transactionId)
        {
            var transaction = await GetAndAuthorizeTransactionAsync(transactionId,userId);
            _repo.Delete(transaction);
            await unitOfWork.CompleteAsync();
        }


        public async Task UpdateTransactionAsync(int userId, UpdateTransactionDTO dto)
        {
            var transaction = await GetAndAuthorizeTransactionAsync(dto.Id, userId);

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
                    wallet.UndoTransaction(transaction.Type, transaction.MoneySource, transaction.Amount.Amount);

                    if (dto.Type == TransactionType.Expense)
                    {
                        await SyncBudgetAsync(userId,transaction.CategoryId, (transaction.Amount.Amount)*-1);
                    }
                    decimal newAmount = transaction.Amount.Amount;
                    if(dto.Amount.HasValue)
                    {
                        newAmount = dto.Amount.Value.Amount;
                    }
                    var type = dto.Type ?? transaction.Type;
                    var source = dto.MoneySource ?? transaction.MoneySource;
                    var categoryId =  dto.CategoryId ?? transaction.CategoryId;
                    wallet.ApplyTransaction(type, source, newAmount,categoryId,"transacation update",true);
                    if (dto.Type == TransactionType.Expense)
                    {
                        await SyncBudgetAsync(userId, categoryId, newAmount);
                    }
                }


                _mapper.Map(dto, transaction);

                _repo.Update(transaction);
                await unitOfWork.CompleteAsync();
                await tran.CommitAsync();

            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                throw;
            }
            

        }
        private async Task<Domain.Entities.Transaction> GetAndAuthorizeTransactionAsync(int transactionId,int userId)
        {
            var transaction = await _repo.GetByIdAsync(transactionId, t => t.Wallet!,t=>t.Category!);
            if (transaction is null) throw new EntityNotFoundException("transaction");
            if (transaction.UserId != userId) throw new UnAuthorizedException("Not authorized");

            return transaction;
        }

        private async Task SyncBudgetAsync(int userId, int? categoryId, decimal amountDelta)
        {
            if (categoryId == null) return;

            var budgetRepo = unitOfWork.Repository<Budget>();
            var budgets = await budgetRepo.GetAsync(b => b.UserId == userId && b.CategoryId == categoryId);
            var activeBudget = budgets.FirstOrDefault();

            if (activeBudget != null)
            {
                activeBudget.Spent = activeBudget.Spent with
                {
                    Amount = activeBudget.Spent.Amount + amountDelta
                };
                budgetRepo.Update(activeBudget);
            }
        }
    }
}
