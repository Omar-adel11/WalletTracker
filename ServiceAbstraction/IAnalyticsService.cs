using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceAbstraction.DTOs.Analytics;

namespace ServiceAbstraction
{
    public interface IAnalyticsService
    {
        Task<DashboardDTO> GetDashboardAsync(int userId,int WalletId,DateTimeOffset? from = null,DateTimeOffset? to = null);
        Task<List<CategorySpendingDTO>> GetCategorySpendingsAsync(int userId, int walletId, DateTimeOffset? from = null, DateTimeOffset? to = null);
        Task<List<MonthlyTrendDTO>> GetMonthlyTrendsAsync(int userId, int walletId, int monthBack = 6);

    }
}
