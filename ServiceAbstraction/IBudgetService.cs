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
        Task<IEnumerable<BudgetDTO>> GetBudgetsByUserId(int userId);
        Task<int> CreateBudget(int userId, decimal amount, int categoryId);
        Task<int> DeleteBudget(int budgetId);
        Task<int> UpdateBudget(int budgetId, decimal? newAmount, int? newCategoryId);
    }
}
