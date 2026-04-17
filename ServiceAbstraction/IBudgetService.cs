using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceAbstraction.DTOs.BudgetDTOs;

namespace ServiceAbstraction
{
    public interface IBudgetService
    {
        Task<IEnumerable<BudgetDTO>> GetBudgetsByUserIdAsync(int userId);
        Task<BudgetDTO> GetBudgetAsync(int BudgetId);
        Task<int> SpendAsync(int budgetId, decimal amount);
        Task<BudgetDTO> CreateBudgetAsync(int userId, decimal amount, int categoryId);
        Task DeleteBudgetAsync(int budgetId);
        Task UpdateBudgetAsync(int budgetId, decimal? newAmount, int? newCategoryId);
    }
}
