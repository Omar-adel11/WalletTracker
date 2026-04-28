namespace ServiceAbstraction.DTOs.Analytics
{
    public class BudgetHealthDTO
    {
        public int TotalBudgets { get; set; }
        public int HealthyBudgets { get; set; }   // < 80% spent
        public int WarningBudgets { get; set; }   // 80-100%
        public int ExceededBudgets { get; set; }  // > 100%
    }
}
