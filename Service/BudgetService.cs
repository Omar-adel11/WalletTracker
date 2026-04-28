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

namespace Service
{
    public class BudgetService(IUnitOfWork _unitOfWork,
        IMapper _mapper) : IBudgetService
    {
        private IGenericRepository<Budget> _repo = _unitOfWork.Repository<Budget>();
        public async Task<BudgetDTO> GetBudgetAsync(int BudgetId, int userId)
        {
            Budget? budget = await GetAndAuthorizeBudget(BudgetId, userId);
            var budgetDTO = _mapper.Map<BudgetDTO>(budget);
            return budgetDTO;
        }

        private async Task<Budget?> GetAndAuthorizeBudget(int BudgetId, int userId)
        {
            var budget = await _repo.GetByIdAsync(BudgetId, b => b.Category!, b => b.Wallet!);
            if (budget is null) throw new EntityNotFoundException("Budget");
            if (budget.Wallet.UserId != userId) throw new EntityNotFoundException($"Budget with id :{BudgetId} for this user");
            return budget;
        }

        public async Task<IEnumerable<BudgetDTO>> GetBudgetsByUserIdAsync(int userId)
        {
            var budgets = await _repo.GetAsync(b => b.UserId == userId,
                                               b => b.Category!, b => b.Wallet!);
            if (!budgets.Any()) throw new EntityNotFoundException("Budget");
            var budgetDTOs = _mapper.Map<IEnumerable<BudgetDTO>>(budgets);
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
            var budget = await _repo.GetByIdAsync(budgetId,b=>b.Wallet);
            if(budget is null) throw new EntityNotFoundException("Budget");
            if (budget.Wallet.UserId != userId) throw new EntityNotFoundException($"Budget with id :{budgetId} for this user");
            _repo.Delete(budget);
            await _unitOfWork.CompleteAsync();
        }

        

        public async Task<bool> SpendAsync(int budgetId,int userId, decimal amount, MoneySource source)
        {
            // 1. Fetch budget with Wallet included (to update balance)
            var budget = await _repo.GetByIdAsync(budgetId, b => b.Wallet!);
            if (budget is null) throw new EntityNotFoundException("Budget");
            if (budget.Wallet.UserId != userId) throw new EntityNotFoundException($"Budget with id :{budgetId} for this user");

            // 2. Check if the budget has enough "Room" left
            if ((budget.Limit.Amount - budget.Spent.Amount) < amount) return false;

            // 3. Check if the Wallet has enough actual money
            var wallet = budget.Wallet;
            if (source == MoneySource.Cash && wallet.Cash < amount) return false;
            if (source == MoneySource.Credit && wallet.Credit < amount) return false;

            // 4. Update the Budget progress
            budget.Spent = budget.Spent with { Amount = budget.Spent.Amount + amount };

            // 5. Update the Wallet balance
            if (source == MoneySource.Cash) wallet.Cash -= amount;
            else wallet.Credit -= amount;

            // 6. Create a Transaction record automatically
            var transaction = new Transaction
            {
                WalletId = budget.WalletId,
                UserId = budget.UserId,
                CategoryId = budget.CategoryId, // Link to the same category as the budget
                Amount = new Money { Amount = amount, Currency = budget.Limit.Currency },
                Description = $"Budget Spend: {budget.Category?.Name ?? "General"}",
                Type = TransactionType.Expense,
                MoneySource = source,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.Repository<Transaction>().AddAsync(transaction);

            // 7. Atomic Save: Everything updates together or nothing does
            _repo.Update(budget);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task UpdateBudgetAsync(int userId, UpdateBudgetDTO updateBudgetDTO)
        {
            // 1. Fetch the existing tracked entity
            var budget = await _repo.GetByIdAsync(updateBudgetDTO.Id,b=>b.Wallet);
            if (budget is null) throw new EntityNotFoundException("Budget");
            if (budget.Wallet.UserId != userId) throw new EntityNotFoundException($"Budget with id :{updateBudgetDTO.Id} for this user");

            // 2. Map the DTO values ONTO the existing tracked entity
            _mapper.Map(updateBudgetDTO, budget);

            // 3. Update audit fields and save
            budget.UpdatedAt = DateTimeOffset.UtcNow;
            _repo.Update(budget);
            await _unitOfWork.CompleteAsync();
        }
    }
}
