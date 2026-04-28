namespace ServiceAbstraction.DTOs.Analytics
{
    public class FinancialSummaryDTO
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetSavings { get; set; }
        public decimal SavingsRate { get; set; } // percentage
        public string Currency { get; set; }
    }
}
