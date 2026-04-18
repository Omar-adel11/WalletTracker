using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceAbstraction.DTOs.ItemToBuyDTOs;

namespace ServiceAbstraction
{
    public interface IItemToBuyService
    {
        Task<IEnumerable<ItemToBuyDTO>> GetAllItemsToBuyAsync(int UserId);
        Task<ItemToBuyDTO> GetAllItemToBuyByIdAsync(int Id);
        Task<ItemToBuyDTO> AddItemAsync(CreateItemToBuyDTO createItemToBuy);
        Task DeleteItemAsync(int itemId);
        Task UpdateItemAsync(UpdateItemToBuyDTO updateItemToBuyDTO);
        Task<bool> SaveMoneyASync(int itemId, decimal saved, int walletId,string Source);
        Task<bool> CompletePurchaseAsync(int itemId);
    }
}
