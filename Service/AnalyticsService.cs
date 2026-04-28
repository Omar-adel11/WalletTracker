using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Entities;
using Domain.Entities.Enum;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.Analytics;

namespace Service
{
    public class AnalyticsService(IUnitOfWork _unitOfWork) : IAnalyticsService
    {
        public async Task<DashboardDTO> GetDashboardAsync(int userId, int WalletId, DateTimeOffset? from = null, DateTimeOffset? to = null)
        {
            var fromDate = from ?? DateTimeOffset.UtcNow.AddMonths(-1);
            var toDate = to ?? DateTimeOffset.UtcNow;

            var transaction = await _unitOfWork.Repository<Domain.Entities.Transaction>()
                                                .GetAsync(t => t.UserId == userId
                                                          && t.WalletId == WalletId
                                                          && t.Date >= fromDate
                                                          && t.Date <= toDate,
                                                          t => t.Category!);
            var wallet = await _unitOfWork.Repository<Wallet>()
             .GetByIdAsync(WalletId);

            var income = transaction.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount.Amount);
            var expense = transaction.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount.Amount);

            var netSavings = income - expense;

            List<CategorySpendingDTO> categorySpending = CategorySpending(transaction, expense);

            var BugdetRepo = _unitOfWork.Repository<Budget>();
            var budgets = await BugdetRepo.GetAsync(b => b.UserId == userId && b.WalletId == WalletId);
            var bugdetHealth = new BudgetHealthDTO
            {
                TotalBudgets = budgets.Count,
                HealthyBudgets = budgets.Count(b => b.Limit.Amount > 0 && b.Spent.Amount < (b.Limit.Amount) * 0.8m),
                WarningBudgets = budgets.Count(b => b.Limit.Amount > 0 && b.Spent.Amount >= (b.Limit.Amount) * 0.8m),
                ExceededBudgets = budgets.Count(b => b.Limit.Amount > 0 && b.Spent.Amount > (b.Limit.Amount))
            };

            var installments = await _unitOfWork.Repository<Installments>()
           .GetAsync(i => i.UserId == userId && !i.IsDone);

            var upcomingInstallments = installments
                .OrderBy(i => i.LastDate)
                .Take(5)
                .Select(i => new UpcomingInstallmentDTO
                {
                    Description = i.Description ?? "Installment",
                    Amount = i.Amount.Amount,
                    Currency = i.Amount.Currency,
                    NextPaymentDate = i.LastDate.AddMonths(1),
                    RemainingPayments = i.totalInstallments - (i.NoOfPaidInstallments ?? 0)
                })
                .ToList();

            return new DashboardDTO
            {
                FinancialSummary = new FinancialSummaryDTO
                {
                    TotalIncome = income,
                    TotalExpenses = expense,
                    NetSavings = netSavings,
                    SavingsRate = netSavings > 0
                    ? Math.Round(netSavings / income * 100, 2)
                    : 0,
                    Currency = wallet?.Currency ?? "EGP"
                },
                TopCategories = categorySpending,
                BudgetHealth = bugdetHealth,
                UpcomingInstallments = upcomingInstallments,
                MonthlyTrends = await GetMonthlyTrendsAsync(userId, WalletId)
            };
        }

        

        public async Task<List<CategorySpendingDTO>> GetCategorySpendingsAsync(int userId, int walletId, DateTimeOffset? from = null, DateTimeOffset? to = null)
        {

            var transactions = await _unitOfWork.Repository<Transaction>()
            .GetAsync(t => t.UserId == userId
                       && t.WalletId == walletId
                       && t.Type == TransactionType.Expense
                       && t.Date >= from && t.Date <= to,
                       t => t.Category!);
            var expense = transactions.Sum(t => t.Amount.Amount);
            return CategorySpending(transactions, expense);
        }

        public async Task<List<MonthlyTrendDTO>> GetMonthlyTrendsAsync(int userId, int walletId, int monthsBack = 6)
        {
            var from = DateTimeOffset.UtcNow.AddMonths(-monthsBack);
            var transactions = await _unitOfWork.Repository<Domain.Entities.Transaction>()
                .GetAsync(t => t.UserId == userId
                           && t.WalletId == walletId
                           && t.Date >= from);

            return transactions
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new MonthlyTrendDTO
                {
                    Month = new DateTimeOffset(g.Key.Year, g.Key.Month, 1, 0, 0, 0, TimeSpan.Zero)
                        .ToString("MMM yyyy"),
                    Income = g.Where(t => t.Type == TransactionType.Income)
                              .Sum(t => t.Amount.Amount),
                    Expenses = g.Where(t => t.Type == TransactionType.Expense)
                                .Sum(t => t.Amount.Amount)
                })
                .ToList();
        }

    private static List<CategorySpendingDTO> CategorySpending(IReadOnlyList<Transaction> transaction, decimal expense)
        {
            return transaction.Where(t => t.Type == TransactionType.Expense && t.Category != null)
                                              .GroupBy(t => t.Category!.Name)
                                              .Select(g => new CategorySpendingDTO
                                              {
                                                  CategoryName = g.Key,
                                                  Amount = g.Sum(t => t.Amount.Amount),
                                                  Percentage = expense > 0
                                                             ? Math.Round(g.Sum(t => t.Amount.Amount) / expense * 100, 2)
                                                             : 0
                                              }).OrderByDescending(c => c.Amount)
                                                .Take(5)
                                                .ToList();
        }
    }

    }