using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceAbstraction.Helper.Email;

namespace ServiceAbstraction
{
    public interface IServiceManager
    {
        IAuthenticationService AuthenticationService { get; }
        IEmailService EmailService { get; }
        IWalletService WalletService { get; }
        ITransactionService TransactionService { get; }
        IBudgetService BudgetService { get; }
        ICategoryService CategoryService { get; }
        IItemToBuyService ItemToBuyService { get; }
        IInstallmentsService InstallmentsService { get; }
        IAnalyticsService AnalyticsService { get; }
    }
}
