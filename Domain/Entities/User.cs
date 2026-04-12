using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Entities.Struct;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class User : IdentityUser<int>, IAduitable
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public Money Balance { get; private set; }
        public DateTimeOffset CreatedAt { get ; set ; }
        public DateTimeOffset UpdatedAt { get; set; }

        //Navigation properties
        public ICollection<Category>? Categories { get; set; } = new HashSet<Category>();
        public ICollection<Transaction>? Transactions { get; set; } = new HashSet<Transaction>();
        public ICollection<ItemToBuy>? ItemsToBuy { get; set; } = new HashSet<ItemToBuy>();
        public ICollection<Budget>? Budgets { get; set; } = new HashSet<Budget>();
        public ICollection<Installments>? Installments { get; set; } = new HashSet<Installments>();

        public void Deposit(Money amount)
        {
            Balance += amount;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
        public void Withdraw(Money amount)
        {
            Balance -= amount;
            UpdatedAt = DateTimeOffset.UtcNow;
        }


    }
}
