using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Struct;

namespace Domain.Entities
{
    public class Budget : BaseEntity
    {
        public string Name { get; set; } = null!;
        public Money Limit { get; set; }
        public Money Spent { get; set; }
        public bool IsDisabled { get; set; } = false;

        //Navigation properties
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        public int WalletId { get; set; }
        public Wallet Wallet { get; set; } = null!;
    }
}
