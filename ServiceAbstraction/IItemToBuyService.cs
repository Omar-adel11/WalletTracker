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
        Task<IEnumerable<ItemToBuyDTO>> GetAllItemsToBuy(int UserId);
        Task<int> AddItem(CreateItemToBuyDTO createItemToBuy);
        Task<int> DeleteItem(int itemId);
        Task<int> UpdateItem(UpdateItemToBuyDTO updateItemToBuyDTO);
        Task<int> SaveMoney(int itemId, decimal saved, int walletId);
    }
}
