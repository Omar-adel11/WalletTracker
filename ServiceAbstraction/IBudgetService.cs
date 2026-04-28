using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Enum;
using ServiceAbstraction.DTOs.BudgetDTOs;
using Shared;

namespace ServiceAbstraction
{
    public interface IBudgetService
    {
        Task<PagedResult<BudgetDTO>> GetBudgetsByUserIdAsync(int userId, int? PageNumber = 1, int? PageSize = 5);
        Task<BudgetDTO> GetBudgetAsync(int BudgetId,int userId);
        Task<bool> SpendAsync(int budgetId,int userId, decimal amount, MoneySource source);
        Task<BudgetDTO> CreateBudgetAsync(CreateBudgetDTO createBudgetDTO);
        Task DeleteBudgetAsync(int budgetId,int userId);
        Task UpdateBudgetAsync(int userId, UpdateBudgetDTO updateBudgetDTO);
    }
}
