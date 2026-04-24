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
        Task<ItemToBuyDTO> GetItemToBuyByIdAsync(int Id,int userId);
        Task<ItemToBuyDTO> AddItemAsync(int userId,CreateItemToBuyDTO createItemToBuy);
        Task DeleteItemAsync(int itemId,int userId);
        Task UpdateItemAsync(int userId,UpdateItemToBuyDTO updateItemToBuyDTO);
        Task<bool> SaveMoneyASync(int userId, SaveMoneyDTO dto);
        Task<bool> CompletePurchaseAsync(int itemId, int userId);
    }
}
