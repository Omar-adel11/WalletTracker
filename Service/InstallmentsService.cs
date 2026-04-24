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
using Domain.Exceptions.AuthExceptions;
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
                throw new EntityNotFoundException("installment");
            }
            var installmentsDTO = _mapper.Map<IEnumerable<InstallmentDTO>>(installments);
            return installmentsDTO;
        }
        
        public async Task<InstallmentDTO> GetInstallmentsByIdAsync(int id,int userId)
        {
            var installment = await _repo.GetByIdAsync(id, i => i.Category!);
            if (installment == null)
                throw new  EntityNotFoundException("installment");
            if(installment.UserId != userId) throw new UnAuthorizedException("You are not authorized");
            var installmentDTO = _mapper.Map<InstallmentDTO>(installment);
            return installmentDTO;
        }
        public async Task<InstallmentDTO> CreateInstallmentAsync(int userId,CreateInstallmentDTO createInstallmentDTO)
        {
            createInstallmentDTO.UserId = userId;
            var wallet = await _unitOfWork.Repository<Wallet>().GetByIdAsync(createInstallmentDTO.WalletId);
            if (wallet == null || wallet.UserId != userId) throw new UnAuthorizedException("you are not authorized");

            var category = await _unitOfWork.Repository<Category>().GetByIdAsync(createInstallmentDTO.CategoryId);
            if (category is null) throw new EntityNotFoundException("cateogory");

            var installment = _mapper.Map<Domain.Entities.Installments>(createInstallmentDTO);
            installment.Category = category;
            installment.Amount = new Money { Amount = createInstallmentDTO.Amount, Currency = wallet.Currency };
            installment.UserId = userId;
            installment.CreatedAt = DateTimeOffset.UtcNow;
            await _repo.AddAsync(installment);
            await _unitOfWork.CompleteAsync();
            var installmentDTO = _mapper.Map<InstallmentDTO>(installment);
            return installmentDTO;
        }

        public async Task DeleteInstallmentAsync(int installmentId, int userId)
        {
            var installment =await _repo.GetByIdAsync(installmentId);
            if (installment is null) throw new EntityNotFoundException("installment");
            if(installment.UserId != userId) throw new UnAuthorizedException($"You are not authorized to delete that installment");
            _repo.Delete(installment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateInstallmentAsync(int userId, int id,UpdateInstallmentDTO updateInstallmentDTO)
        {
            updateInstallmentDTO.Id = id;
            var installment = await _repo.GetByIdAsync(updateInstallmentDTO.Id,i => i.Category!);
            if (installment is null) throw new EntityNotFoundException("installment");
            if (installment.UserId != userId) throw new UnAuthorizedException("you are not authorized to update this installment");

            installment.Description = updateInstallmentDTO.Description ?? installment.Description;
            decimal finalAmount = updateInstallmentDTO.Amount ?? installment.Amount.Amount;
            installment.Amount = installment.Amount with { Amount = finalAmount };

            installment.CategoryId = updateInstallmentDTO.CategoryId ?? installment.CategoryId;
            installment.StartDate = updateInstallmentDTO.StartDate ?? installment.StartDate;
            installment.EndDate = updateInstallmentDTO.EndDate ?? installment.EndDate;
            installment.NoOfPaidInstallments = updateInstallmentDTO.NoOfPaidInstallments ?? installment.NoOfPaidInstallments;

            
            installment.UpdatedAt = DateTimeOffset.UtcNow;
            _repo.Update(installment);
            await _unitOfWork.CompleteAsync();

        }

        public async Task<bool> payInstallmentAsync(int installmentId, PayInstallmentDTO dto)
        {
            var installment = await _repo.GetByIdAsync(installmentId,i=>i.Category!);
            if (installment is null) throw new EntityNotFoundException("installment");

            int totalInstallments = ((installment.EndDate.Year - installment.StartDate.Year) * 12)
                            + installment.EndDate.Month - installment.StartDate.Month + 1;
            if (installment.IsDone)
            {
                throw new EntityNotFoundException("installment");
            }

            var wallet = await _unitOfWork.Repository<Wallet>().GetByIdAsync(dto.WalletId);
            if (wallet is null) throw new EntityNotFoundException("wallet");

            if(installment.Amount.Currency != wallet.Currency) throw new CurrencyMismatchException();

            if (dto.source == MoneySource.Cash)
            {
                if (installment.Amount.Amount > wallet.Cash) throw new NotEnoughBalanceException();
                wallet.Cash -= installment.Amount.Amount;
            }
            else if (dto.source == MoneySource.Credit)
            {
                if (installment.Amount.Amount > wallet.Credit) throw new NotEnoughBalanceException();
                wallet.Credit -= installment.Amount.Amount;
            }
            else throw new InvalidSourceException(dto.source.ToString());

            installment.NoOfPaidInstallments++;
            if (installment.NoOfPaidInstallments == totalInstallments) installment.IsDone = true;
            installment.LastDate = DateTimeOffset.UtcNow;

            var transaction = new Domain.Entities.Transaction
            {
                Amount = installment.Amount,
                Type = TransactionType.Expense,
                Description = "Pay installment",
                Date = DateTimeOffset.UtcNow,
                MoneySource = dto.source,
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
