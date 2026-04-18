using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Entities.Struct;
using Domain.Exceptions.MoneyInvalidOperationException;
using Domain.Exceptions.NullReferenceException;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.InstallmentsDTOs;

namespace Service
{
    public class InstallmentsService(IUnitOfWork _unitOfWork,IMapper _mapper) : IInstallmentsService
    {
        private IGenericRepository<Installments> _repo = _unitOfWork.Repository<Installments>();
        public async Task<IEnumerable<InstallmentDTO>> GetAllInstallmentsAsync(int userId)
        {
            var installments = await _repo.GetAsync(i=>i.UserId ==  userId, i => i.Category!);
            if(!installments.Any())
            {
                throw new InstallmetnsNullException(userId);
            }
            var installmentsDTO = _mapper.Map<IEnumerable<InstallmentDTO>>(installments);
            return installmentsDTO;
        }

        public async Task<InstallmentDTO> GetInstallmentsByIdAsync(int id)
        {
            var installment = await _repo.GetByIdAsync(id, i => i.Category);
            if (installment == null)
                throw new InstallmentNullException(id);
            var installmentDTO = _mapper.Map<InstallmentDTO>(installment);
            return installmentDTO;
        }
        public async Task<InstallmentDTO> CreateInstallmentAsync(CreateInstallmentDTO createInstallmentDTO)
        {
            var installment = _mapper.Map<Installments>(createInstallmentDTO);
            installment.CreatedAt = DateTimeOffset.UtcNow;
            await _repo.AddAsync(installment);
            await _unitOfWork.CompleteAsync();
            var installmentDTO = _mapper.Map<InstallmentDTO>(installment);
            return installmentDTO;
        }

        public async Task DeleteInstallmentAsync(int installmentId)
        {
            var installment =await _repo.GetByIdAsync(installmentId);
            if (installment is null) throw new InstallmentNullException(installmentId);
            _repo.Delete(installment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateInstallmentAsync(UpdateInstallmentDTO updateInstallmentDTO)
        {
            var installment = await _repo.GetByIdAsync(updateInstallmentDTO.Id,i => i.Category!);
            if (installment is null) throw new InstallmentNullException(updateInstallmentDTO.Id);
            _mapper.Map(updateInstallmentDTO, installment);
            installment.UpdatedAt = DateTimeOffset.UtcNow;
            _repo.Update(installment);
            await _unitOfWork.CompleteAsync();

        }

        public async Task<bool> payInstallmentAsync(int installmentId, int wallet_id,MoneySource source)
        {
            var installment = await _repo.GetByIdAsync(installmentId,i=>i.Category!);
            if (installment is null) throw new InstallmentNullException(installmentId);

            int totalInstallments = ((installment.EndDate.Year - installment.StartDate.Year) * 12)
                            + installment.EndDate.Month - installment.StartDate.Month + 1;
            if (installment.IsDone)
            {
                throw new AllInstallmentsPaidException(installmentId);
            }

            var wallet = await _unitOfWork.Repository<Wallet>().GetByIdAsync(wallet_id);
            if (wallet is null) throw new WalletNullException(wallet_id);

            if(installment.Amount.Currency != wallet.Currency) throw new CurrencyMismatchException();

            if (source == MoneySource.Cash)
            {
                if (installment.Amount.Amount > wallet.Cash) throw new NotEnoughBalanceException();
                wallet.Cash -= installment.Amount.Amount;
            }
            else if (source == MoneySource.Credit)
            {
                if (installment.Amount.Amount > wallet.Credit) throw new NotEnoughBalanceException();
                wallet.Credit -= installment.Amount.Amount;
            }
            else throw new InvalidSourceException(source.ToString());

            installment.NoOfPaidInstallments++;
            if (installment.NoOfPaidInstallments == totalInstallments) installment.IsDone = true;
            installment.LastDate = DateTimeOffset.UtcNow;

            var transaction = new Domain.Entities.Transaction
            {
                Amount = installment.Amount,
                Type = TransactionType.Expense,
                Description = "Pay installment",
                Date = DateTimeOffset.UtcNow,
                MoneySource = source,
                UserId = wallet.UserId,
                CategoryId = installment.CategoryId,
                InstallmentsId = installment.id,
                WalletId = wallet.id
            };

            var transactionRepo = _unitOfWork.Repository<Domain.Entities.Transaction>();
            await transactionRepo.AddAsync(transaction);

            var BudgetRepo = _unitOfWork.Repository<Domain.Entities.Budget>();
            var budget = (await BudgetRepo.GetAsync(b => b.UserId == wallet.UserId && b.CategoryId == installment.CategoryId)).FirstOrDefault();
            if(budget != null)
            {
                budget.Spent = budget.Spent with
                {
                    Amount = budget.Spent.Amount + installment.Amount.Amount
                };
                BudgetRepo.Update(budget);
            }

            _repo.Update(installment);
            return await _unitOfWork.CompleteAsync() > 0;
        }
    }
}
