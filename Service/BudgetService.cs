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
using Domain.Exceptions.BadRequestException;
using Domain.Exceptions.NullReferenceException;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.BudgetDTOs;
using Shared;

namespace Service
{
    public class BudgetService(IUnitOfWork _unitOfWork,
        IMapper _mapper) : IBudgetService
    {
        private IGenericRepository<Budget> _repo = _unitOfWork.Repository<Budget>();
        public async Task<BudgetDTO> GetBudgetAsync(int BudgetId, int userId)
        {
            var budget = await GetAndAuthorizeBudget(BudgetId, userId);
            var budgetDTO = _mapper.Map<BudgetDTO>(budget);
            return budgetDTO;
        }


        public async Task<PagedResult<BudgetDTO>> GetBudgetsByUserIdAsync(int userId, int? PageNumber = 1, int? PageSize = 5)
        {
            var budgets = await _repo.GetAsyncFilteredWithPaginate(b => b.UserId == userId,
                                                                   b=>b.CreatedAt,
                                                                   PageNumber,
                                                                   PageSize,
                                                                   b => b.Category!, b => b.Wallet!);
            if (!budgets.Items.Any()) throw new EntityNotFoundException("Budget");
            var budgetDTOs = _mapper.Map<PagedResult<BudgetDTO>>(budgets);
            return budgetDTOs;
        }

        public async Task<BudgetDTO> CreateBudgetAsync(CreateBudgetDTO createBudgetDTO)
        {
            var IsExist = await _unitOfWork.Repository<Budget>().ExistsAsync(b => b.UserId == createBudgetDTO.UserId && b.CategoryId == createBudgetDTO.CategoryId);
            if (!IsExist) throw new CategoryExistException();
            var wallet = await _unitOfWork.Repository<Wallet>().GetByIdAsync(createBudgetDTO.WalletId);
            createBudgetDTO.Currency = wallet.Currency;
            var budget = _mapper.Map<Budget>(createBudgetDTO);
            budget.CreatedAt = DateTimeOffset.UtcNow;
            await _repo.AddAsync(budget);
            await _unitOfWork.CompleteAsync();
            var BudgetDTO = _mapper.Map<BudgetDTO>(budget);
            return BudgetDTO;
        }

        public async Task DeleteBudgetAsync(int budgetId,int userId)
        {
            var budget = await GetAndAuthorizeBudget(budgetId, userId);
            _repo.Delete(budget);
            await _unitOfWork.CompleteAsync();
        }

        

        public async Task<bool> SpendAsync(int budgetId,int userId, decimal amount, MoneySource source)
        {
            // 1. Fetch budget with Wallet included (to update balance)
            var budget = await GetAndAuthorizeBudget(budgetId, userId);

            // 2. Check if the budget has enough "Room" left
            if ((budget.Limit.Amount - budget.Spent.Amount) < amount) return false;

            
           budget.Wallet.ApplyTransaction(TransactionType.Expense,source,amount,budget.CategoryId,$"spend money for budget {budget.Name}");
           budget.Spent = budget.Spent with { Amount = budget.Spent.Amount + amount };

            _repo.Update(budget);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task UpdateBudgetAsync(int userId, UpdateBudgetDTO updateBudgetDTO)
        {
            var budget = await GetAndAuthorizeBudget(updateBudgetDTO.Id, userId);
            _mapper.Map(updateBudgetDTO, budget);

            budget.UpdatedAt = DateTimeOffset.UtcNow;
            _repo.Update(budget);
            await _unitOfWork.CompleteAsync();
        }
        private async Task<Budget?> GetAndAuthorizeBudget(int BudgetId, int userId)
        {
            var budget = await _repo.GetByIdAsync(BudgetId, b => b.Category!, b => b.Wallet!);
            if (budget is null) throw new EntityNotFoundException("Budget");
            if (budget.Wallet.UserId != userId) throw new EntityNotFoundException($"Budget with id :{BudgetId} for this user");
            return budget;
        }

    }
}
