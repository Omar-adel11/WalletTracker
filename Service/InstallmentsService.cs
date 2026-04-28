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
using Shared;

namespace Service
{
    public class InstallmentsService(IUnitOfWork _unitOfWork,IMapper _mapper) : IInstallmentsService
    {
        private IGenericRepository<Installments> _repo = _unitOfWork.Repository<Installments>();
        public async Task<PagedResult<InstallmentDTO>> GetAllInstallmentsAsync(int userId, int? PageNumber = 1, int? PageSize = 5)
        {
            var installments = await _repo.GetAsyncFilteredWithPaginate(i=>i.UserId ==  userId,
                                                                        i=>i.CreatedAt,
                                                                        PageNumber,
                                                                        PageSize,
                                                                        i => i.Category!);
            if(!installments.Items.Any())
            {
                throw new EntityNotFoundException("installment");
            }
            var installmentsDTO = _mapper.Map<PagedResult<InstallmentDTO>>(installments);
            return installmentsDTO;
        }
        
        public async Task<InstallmentDTO> GetInstallmentsByIdAsync(int id,int userId)
        {
            var installment = await GetAndAuthorizeInstallmentAsync(id, userId);
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
            var installment = await GetAndAuthorizeInstallmentAsync(installmentId, userId);
            _repo.Delete(installment);
            await _unitOfWork.CompleteAsync();
        }

        public async Task UpdateInstallmentAsync(int userId, int id,UpdateInstallmentDTO updateInstallmentDTO)
        {
            updateInstallmentDTO.Id = id;
            var installment = await GetAndAuthorizeInstallmentAsync(id, userId);

            _mapper.Map(updateInstallmentDTO, installment);
            
            if (updateInstallmentDTO.Amount.HasValue)
            {
                installment.Amount = installment.Amount with
                {
                    Amount = updateInstallmentDTO.Amount.Value
                };
            }

            installment.UpdatedAt = DateTimeOffset.UtcNow;

            _repo.Update(installment);
            await _unitOfWork.CompleteAsync();

        }

        public async Task<bool> payInstallmentAsync(int installmentId, int userId,PayInstallmentDTO dto)
        {
            var wallet = await _unitOfWork.Repository<Wallet>().GetByIdAsync(dto.WalletId);
            if (wallet == null || wallet.UserId != userId) throw new UnAuthorizedException("you are not authorized");
            var installment = await GetAndAuthorizeInstallmentAsync(installmentId, userId);

            int totalInstallments = ((installment.EndDate.Year - installment.StartDate.Year) * 12)
                            + installment.EndDate.Month - installment.StartDate.Month + 1;
            if (installment.IsDone)
            {
                throw new EntityNotFoundException("installment");
            }

            wallet.ApplyTransaction(TransactionType.Expense, dto.source, installment.Amount.Amount, installment.CategoryId, $"pay installment {installment.Description}");

            installment.NoOfPaidInstallments++;
            if (installment.NoOfPaidInstallments == totalInstallments) installment.IsDone = true;
            installment.LastDate = DateTimeOffset.UtcNow;


            await SyncBudgetAsync(installment.UserId,installment.CategoryId,installment.Amount.Amount);
            

            _repo.Update(installment);
            return await _unitOfWork.CompleteAsync() > 0;
        }
        private async Task<Domain.Entities.Installments> GetAndAuthorizeInstallmentAsync(int installmentId, int userId)
        {
            var installment = await _repo.GetByIdAsync(installmentId, i => i.Category!);
            if (installment is null) throw new EntityNotFoundException("installment");
            if (installment.UserId != userId) throw new UnAuthorizedException("Not authorized");

            return installment;
        }

        private async Task SyncBudgetAsync(int userId, int? categoryId, decimal amountDelta)
        {
            if (categoryId == null) return;

            var budgetRepo = _unitOfWork.Repository<Budget>();
            var budgets = await budgetRepo.GetAsync(b => b.UserId == userId && b.CategoryId == categoryId);
            var activeBudget = budgets.FirstOrDefault();

            if (activeBudget != null)
            {
                activeBudget.Spent = activeBudget.Spent with
                {
                    Amount = activeBudget.Spent.Amount + amountDelta
                };
                budgetRepo.Update(activeBudget);
            }
        }
    }
}
