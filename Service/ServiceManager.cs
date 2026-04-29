using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using ServiceAbstraction;
using ServiceAbstraction.Helper.Email;

namespace Service
{
    public class ServiceManager : IServiceManager
    {
        private readonly Lazy<IAuthenticationService> _authService;
        private readonly Lazy<IEmailService> _emailService;
        private readonly Lazy<IWalletService> _walletService;
        private readonly Lazy<ITransactionService> _transactionService;
        private readonly Lazy<IBudgetService> _budgetService;
        private readonly Lazy<ICategoryService> _categoryService;
        private readonly Lazy<IItemToBuyService> _itemToBuyService;
        private readonly Lazy<IInstallmentsService> _installmentsService;
        private readonly Lazy<IAnalyticsService> _analyticsService;
        private readonly Lazy<ICacheService> _cacheService;

        public ServiceManager(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<User> userManager,
            IConfiguration config,
            IEmailService emailService, IWebHostEnvironment _environment,
            ICacheRepository cacheRepository) // EmailService injected from DI
        {
            _authService = new Lazy<IAuthenticationService>(() => new AuthenticationService(mapper, userManager, config, emailService, unitOfWork,_environment));
            _emailService = new Lazy<IEmailService>(() => emailService);
            _walletService = new Lazy<IWalletService>(() => new WalletService(unitOfWork, mapper));
            _transactionService = new Lazy<ITransactionService>(() => new TransactionService(unitOfWork, mapper));
            _budgetService = new Lazy<IBudgetService>(() => new BudgetService(unitOfWork, mapper));
            _categoryService = new Lazy<ICategoryService>(() => new CategoryService(unitOfWork, mapper));
            _itemToBuyService = new Lazy<IItemToBuyService>(() => new ItemToBuyService(unitOfWork, mapper));
            _installmentsService = new Lazy<IInstallmentsService>(() => new InstallmentsService(unitOfWork, mapper));
            _analyticsService = new Lazy<IAnalyticsService>(() => new AnalyticsService(unitOfWork));
            _cacheService = new Lazy<ICacheService>(() => new CacheService(cacheRepository));
        }

        public IAuthenticationService AuthenticationService => _authService.Value;
        public IEmailService EmailService => _emailService.Value;
        public IWalletService WalletService => _walletService.Value;
        public ITransactionService TransactionService => _transactionService.Value;
        public IBudgetService BudgetService => _budgetService.Value;
        public ICategoryService CategoryService => _categoryService.Value;
        public IItemToBuyService ItemToBuyService => _itemToBuyService.Value;
        public IInstallmentsService InstallmentsService => _installmentsService.Value;
        public IAnalyticsService AnalyticsService => _analyticsService.Value;
        public ICacheService CacheService => _cacheService.Value;
    }
}
