using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Enum;

namespace ServiceAbstraction.DTOs.ItemToBuyDTOs
{
    public class SaveMoneyDTO
    {
        public int Id { get; set; }
        public int walletId {  get; set; }  
        public decimal amount { get; set; }
        public MoneySource source {  get; set; }

    }
}
