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
using ServiceAbstraction.DTOs.ItemToBuyDTOs;

namespace Service
{
    public class ItemToBuyService(IUnitOfWork _unitOfWork, IMapper _mapper) : IItemToBuyService
    {
        IGenericRepository<ItemToBuy> _repo = _unitOfWork.Repository<ItemToBuy>();

        public async Task<IEnumerable<ItemToBuyDTO>> GetAllItemsToBuyAsync(int UserId)
        {
            var Items = await _repo.GetAsync(i => i.UserId == UserId, i => i.Category!, i => i.Wallet!);
            if (!Items.Any()) throw new EntityNotFoundException("Item");
            return _mapper.Map<IEnumerable<ItemToBuyDTO>>(Items);

        }

        public async Task<ItemToBuyDTO> GetItemToBuyByIdAsync(int Id,int userId)
        {
            var Item = await _repo.GetByIdAsync(Id, i => i.Category!, i => i.Wallet!);
            if (Item is null) throw new EntityNotFoundException("Item");
            if(Item.UserId !=  userId) throw new UnAuthorizedException("you are not authorized to get this item");
            return _mapper.Map<ItemToBuyDTO>(Item);
        }

        public async Task<ItemToBuyDTO> AddItemAsync(int userId,CreateItemToBuyDTO createItemToBuy)
        {
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
            var item = await _repo.GetByIdAsync(itemId, i => i.Wallet!,i => i.Category!);
            if (item is null) throw new EntityNotFoundException("Item");
            if (item.UserId != userId) throw new UnAuthorizedException("you are not authorized to do purchase this item");
            if (!item.IsAchieved) throw new ItemToBuyBalanceException();

            item.Wallet.Pended -= item.Price.Amount;

            var transaction = new Transaction
            {
                WalletId = item.WalletId,
                UserId = item.UserId,
                CategoryId = item.CategoryId,
                Amount = item.Price, // The final cost
                Description = $"Purchased Item: {item.Name}",
                Type = TransactionType.Expense,
                Date = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.Repository<Transaction>().AddAsync(transaction);
            
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task DeleteItemAsync(int itemId,int userId)
        {
            var Item = await _repo.GetByIdAsync(itemId);
            if (Item is null) throw new EntityNotFoundException("Item");
            if (Item.UserId != userId) throw new UnAuthorizedException("you are not authorized to delete this item");
            var walletRepo =  _unitOfWork.Repository<Wallet>();
            var wallet = await walletRepo.GetByIdAsync(Item.WalletId);
            if (wallet is null) throw new EntityNotFoundException("wallet");
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                wallet.Pended -= Item.Amount.Amount;
                wallet.Cash += Item.Amount.Amount;
                _repo.Delete(Item);
                walletRepo.Update(wallet);
                
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
            var item = await _repo.GetByIdAsync(updateItemToBuyDTO.Id,i=>i.Category!,id=>id.Wallet!);
            if(item is null) throw new EntityNotFoundException("Item");
            if (item.UserId != userId) throw new UnAuthorizedException("you are not authorized to update this item");
            item.CategoryId = updateItemToBuyDTO.CategoryId ?? item.CategoryId;
            item.Name = updateItemToBuyDTO.Name ?? item.Name;
            if(updateItemToBuyDTO.Price.HasValue)
            {
                item.Price = new Money(updateItemToBuyDTO.Price.Value,item.Price.Currency);
            }
            item.UpdatedAt = DateTimeOffset.UtcNow;
            _repo.Update(item);
            await _unitOfWork.CompleteAsync();
        }
       
        public async Task<bool> SaveMoneyASync(int userId,SaveMoneyDTO dto)
        {
            var item = await _repo.GetByIdAsync(dto.Id, i => i.Wallet!);
            if (item is null) throw new EntityNotFoundException("Item");

            if (item.Wallet.id != dto.walletId) return false;
            if(item.IsAchieved) return false; // Can't save money for an already achieved item
            if(item.UserId != userId) throw new UnAuthorizedException("you are not authorized to save money for this item");
            bool isCash = dto.source == MoneySource.Cash;
            bool isCredit = dto.source == MoneySource.Credit;

            // 1. Validation & Initial Deduction
            if (!isCash && !isCredit) return false;
            if (isCash && item.Wallet.Cash < dto.amount) return false;
            if (isCredit && item.Wallet.Credit < dto.amount) return false;


            decimal remaining = item.Price.Amount - item.Amount.Amount;
            if (remaining < 0)
            {
                item.IsAchieved = true;
                return false;
            }
            decimal amountToSave = Math.Min(dto.amount, remaining);
            decimal change = dto.amount - amountToSave;

           
            if (isCash) item.Wallet.Cash -= amountToSave;
            else item.Wallet.Credit -= amountToSave;

            
            item.Amount = item.Amount with { Amount = item.Amount.Amount + amountToSave };
            item.Wallet.Pended += amountToSave;

            // 3. Achievement & Change Logic
            if (item.Amount.Amount >= item.Price.Amount)
            {
                item.IsAchieved = true;
              
            }


            if (change > 0)
            {

                if (isCash) item.Wallet.Cash += change;
                else item.Wallet.Credit += change;


            }
            item.UpdatedAt = DateTimeOffset.UtcNow;
            return await _unitOfWork.CompleteAsync() > 0;
        }

    }
}
