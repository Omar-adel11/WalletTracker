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
using Domain.Exceptions.BadRequestException;
using Domain.Exceptions.MoneyInvalidOperationException;
using Domain.Exceptions.NullReferenceException;
using Service.Helper;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.InstallmentsDTOs;
using ServiceAbstraction.DTOs.ItemToBuyDTOs;
using ServiceAbstraction.DTOs.WalletsDtos;
using Shared;

namespace Service
{
    public class ItemToBuyService(IUnitOfWork _unitOfWork, IMapper _mapper) : IItemToBuyService
    {
        IGenericRepository<ItemToBuy> _repo = _unitOfWork.Repository<ItemToBuy>();

        public async Task<PagedResult<ItemToBuyDTO>> GetAllItemsToBuyAsync(int UserId, int? PageNumber = 1, int? PageSize = 5)
        {
            var Items = await _repo.GetAsyncFilteredWithPaginate(i => i.UserId == UserId,
                                                                 i=>i.CreatedAt,
                                                                 PageNumber,
                                                                 PageSize,
                                                                 i => i.Category!, i => i.Wallet!);
            if (!Items.Items.Any()) throw new EntityNotFoundException("Item");
            return _mapper.Map<PagedResult<ItemToBuyDTO>>(Items);

        }

        public async Task<ItemToBuyDTO> GetItemToBuyByIdAsync(int Id,int userId)
        {
            var Item = await GetAndAuthorizeItem(userId, Id);
            return _mapper.Map<ItemToBuyDTO>(Item);
        }

        public async Task<ItemToBuyDTO> AddItemAsync(int userId,CreateItemToBuyDTO createItemToBuy)
        {
            var user = await _unitOfWork.Repository<User>()
                              .GetFirstOrDefaultAsync(u => u.Id == userId);
            

            if (user is null) throw new UserNotFoundNullException();

            if (!user.IsPremium)
            {
                var ItemCount = await _repo.CountAsync(w => w.UserId == userId);
                if (ItemCount >= PlanLimits.FreeWalletLimit)
                    throw new LimitExceededException(
                        $"Items");
            }

            createItemToBuy.UserId = userId;
            var Item = _mapper.Map<ItemToBuy>(createItemToBuy);
            var wallet = await _unitOfWork.Repository<Wallet>().GetByIdAsync(createItemToBuy.WalletId);
            if (wallet is null) throw new EntityNotFoundException("wallet");
            wallet.Pended += createItemToBuy.Amount.Amount;
          
            Item.CreatedAt = DateTimeOffset.UtcNow;
            await _repo.AddAsync(Item);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<ItemToBuyDTO>(Item); 
        }

        public async Task<bool> CompletePurchaseAsync(int itemId,int userId)
        {
            var Item = await GetAndAuthorizeItem(userId, itemId);
            if (!Item.IsAchieved) throw new ItemToBuyBalanceException();

            Item.Wallet.ApplyTransaction(TransactionType.Expense, MoneySource.Pended,Item.Price.Amount, Item.CategoryId.Value, $"purchase {Item.Name}");
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task DeleteItemAsync(int itemId,int userId)
        {
            var Item = await GetAndAuthorizeItem(userId, itemId);
           
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                Item.Wallet.ApplyTransaction(TransactionType.Income, MoneySource.Cash, Item.Amount.Amount, Item.CategoryId.Value, $"return the saved money for {Item.Name}");
                Item.Wallet.ApplyTransaction(TransactionType.Expense, MoneySource.Pended, Item.Amount.Amount, Item.CategoryId.Value, $"return the saved money for {Item.Name}");

                await _unitOfWork.CompleteAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw ;
            }
        
        }

        public async Task UpdateItemAsync(int userId, UpdateItemToBuyDTO updateItemToBuyDTO)
        {
            var Item = await GetAndAuthorizeItem(userId, updateItemToBuyDTO.Id);
            Item.CategoryId = updateItemToBuyDTO.CategoryId ?? Item.CategoryId;
            Item.Name = updateItemToBuyDTO.Name ?? Item.Name;
            if(updateItemToBuyDTO.Price.HasValue)
            {
                Item.Price = new Money(updateItemToBuyDTO.Price.Value, Item.Price.Currency);
            }
            Item.UpdatedAt = DateTimeOffset.UtcNow;
            _repo.Update(Item);
            await _unitOfWork.CompleteAsync();
        }
       
        public async Task<bool> SaveMoneyASync(int userId,SaveMoneyDTO dto)
        {
            var Item = await GetAndAuthorizeItem(userId, dto.Id);
            if (Item.Wallet.id != dto.walletId) return false;
            if(Item.IsAchieved) return false; // Can't save money for an already achieved item

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {

                decimal remaining = Item.Price.Amount - Item.Amount.Amount;
                if (remaining < 0)
                {
                    Item.IsAchieved = true;
                    return false;
                }
                decimal amountToSave = Math.Min(dto.amount, remaining);
                decimal change = dto.amount - amountToSave;


                Item.Wallet.ApplyTransaction(TransactionType.Expense, MoneySource.Cash, amountToSave, Item.CategoryId.Value, $"save money for {Item.Name}");
                Item.Wallet.ApplyTransaction(TransactionType.Income, MoneySource.Pended, amountToSave, Item.CategoryId.Value, $"save saved money for {Item.Name}");


                if (Item.Amount.Amount >= Item.Price.Amount)
                {
                    Item.IsAchieved = true;

                }

                Item.UpdatedAt = DateTimeOffset.UtcNow;

                await _unitOfWork.CompleteAsync();
                await transaction.CommitAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }

          
        }

        private async Task<ItemToBuy> GetAndAuthorizeItem(int userId,int itemId)
        {
            var Item = await _repo.GetByIdAsync(itemId, i => i.Category!, i => i.Wallet!);
            if (Item is null) throw new EntityNotFoundException("Item");
            if (Item.UserId != userId) throw new UnAuthorizedException("you are not authorized to get this item");
            return Item;
        }
    }
}
