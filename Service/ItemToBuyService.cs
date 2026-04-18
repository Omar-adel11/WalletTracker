using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Exceptions.MoneyInvalidOperationException;
using Domain.Exceptions.NullReferenceException;
using ServiceAbstraction;
using ServiceAbstraction.DTOs.ItemToBuyDTOs;

namespace Service
{
    public class ItemToBuyService(IUnitOfWork _unitOfWork, IMapper _mapper) : IItemToBuyService
    {
        IGenericRepository<ItemToBuy> _repo = _unitOfWork.Repository<ItemToBuy>();
        public async Task<ItemToBuyDTO> AddItemAsync(CreateItemToBuyDTO createItemToBuy)
        {
            var Item = _mapper.Map<ItemToBuy>(createItemToBuy);
            Item.CreatedAt = DateTimeOffset.UtcNow;
            await _repo.AddAsync(Item);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<ItemToBuyDTO>(Item); 
        }

        public async Task<bool> CompletePurchaseAsync(int itemId)
        {
            var item = await _repo.GetByIdAsync(itemId, i => i.Wallet!,i => i.Category!);

            if (item is null) throw new ItemNullException(itemId);
            if(!item.IsAchieved) throw new ItemToBuyBalanceException();

            item.Wallet.Pended -= item.Price.Amount;

            var transaction = new Transaction
            {
                WalletId = item.WalletId,
                UserId = item.UserId,
                CategoryId = item.CategoryId,
                Amount = item.Price, // The final cost
                Description = $"Purchased Item: {item.Name}",
                Type = TransactionType.Expense,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.Repository<Transaction>().AddAsync(transaction);
            _repo.Delete(item);
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task DeleteItemAsync(int itemId)
        {
            var Item = await _repo.GetByIdAsync(itemId);
            if (Item is null) throw new ItemNullException(itemId);
            _repo.Delete(Item);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<ItemToBuyDTO>> GetAllItemsToBuyAsync(int UserId)
        {
            var Items = await _repo.GetAsync(i => i.UserId == UserId, i => i.Category!, i => i.Wallet!);
            if(!Items.Any()) throw new ItemsNullException(UserId);
            return _mapper.Map<IEnumerable<ItemToBuyDTO>>(Items);

        }

        public async Task<ItemToBuyDTO> GetAllItemToBuyByIdAsync(int Id)
        {
            var Item = await _repo.GetByIdAsync(Id, i => i.Category!, i => i.Wallet!);
            if (Item is null) throw new ItemNullException(Id);
            return _mapper.Map<ItemToBuyDTO>(Item);
        }

        public async Task<bool> SaveMoneyASync(int itemId, decimal saved, int walletId,string source)
        {
            var item = await _repo.GetByIdAsync(itemId, i => i.Wallet!);
            if (item is null) throw new ItemNullException(itemId);
            if (item.Wallet.id != walletId) return false;
            if(item.IsAchieved) return false; // Can't save money for an already achieved item

            bool isCash = source.Equals("cash", StringComparison.OrdinalIgnoreCase);
            bool isCredit = source.Equals("credit", StringComparison.OrdinalIgnoreCase);

            // 1. Validation & Initial Deduction
            if (isCash && item.Wallet.Cash < saved) return false;
            if (isCredit && item.Wallet.Credit < saved) return false;
            if (!isCash && !isCredit) return false;

            if (isCash) item.Wallet.Cash -= saved;
            else item.Wallet.Credit -= saved;

            // 2. Update Progress (Apply the full saved amount first)
            item.Amount = item.Amount with { Amount = item.Amount.Amount + saved };
            item.Wallet.Pended += saved;

            // 3. Achievement & Change Logic
            if (item.Amount.Amount >= item.Price.Amount)
            {
                item.IsAchieved = true;
                decimal change = item.Amount.Amount - item.Price.Amount;

                if (change > 0)
                {
                    // Return change to source
                    if (isCash) item.Wallet.Cash += change;
                    else item.Wallet.Credit += change;

                    // Correct the Pended balance (Remove the extra change)
                    item.Wallet.Pended -= change;

                    // Sync the item amount to exactly the price so it's clean in the DB
                    item.Amount = item.Amount with { Amount = item.Price.Amount };
                }
            }

            item.UpdatedAt = DateTimeOffset.UtcNow;
            return await _unitOfWork.CompleteAsync() > 0;
        }

        public async Task UpdateItemAsync(UpdateItemToBuyDTO updateItemToBuyDTO)
        {
            var item = await _repo.GetByIdAsync(updateItemToBuyDTO.Id,i=>i.Category!,id=>id.Wallet!);
            if(item is null) throw new ItemNullException(updateItemToBuyDTO.Id);
            _mapper.Map(updateItemToBuyDTO, item);  
            item.UpdatedAt = DateTimeOffset.UtcNow;
            _repo.Update(item);
            await _unitOfWork.CompleteAsync();
        }
    }
}
