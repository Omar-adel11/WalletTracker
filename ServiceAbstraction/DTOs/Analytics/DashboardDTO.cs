using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.DTOs.Analytics
{
    // ServiceAbstraction/DTOs/Analytics/DashboardDTO.cs
    public class DashboardDTO
    {
        public FinancialSummaryDTO FinancialSummary { get; set; }
        public List<CategorySpendingDTO> TopCategories { get; set; }
        public List<MonthlyTrendDTO> MonthlyTrends { get; set; }
        public BudgetHealthDTO BudgetHealth { get; set; }
        public List<UpcomingInstallmentDTO> UpcomingInstallments { get; set; }
    }
}
