using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Enum;
using Domain.Entities.Struct;

namespace Domain.Entities
{
    public class Wallet : BaseEntity
    {
        public string Currency { get; set; } = "EGP";
        public decimal TotalBalance => Cash + Credit + Pended;
        public decimal Cash { get; set; } = 0;
        public decimal Credit { get; set; } = 0;
        public decimal Pended { get; set; } = 0;


        public int UserId { get; set; }
        public User user { get; set; } 

        public void Deposit(decimal amount, BalanceSource source)
        {
            if (source == BalanceSource.Cash) Cash += amount;
            else if (source == BalanceSource.Credit) Credit += amount;

            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Withdraw(decimal amount, BalanceSource source)
        {
            
            if (source == BalanceSource.Cash && Cash < amount)
                throw new InvalidOperationException("Insufficient Cash funds");

            if (source == BalanceSource.Cash) Cash -= amount;
            else if (source == BalanceSource.Credit) Credit -= amount;

            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void PendMoney(decimal amount,BalanceSource source)
        {
            if (Cash < amount) throw new InvalidOperationException("Not enough cash to pend");
            if (source == BalanceSource.Cash)
                Cash = Cash - amount;
            else if (source == BalanceSource.Credit)
                Credit = Credit-amount;
            Pended =Pended+ amount;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        //Navegation properties
        public ICollection<Transaction>? Transactions { get; set; } = new HashSet<Transaction>();
        public ICollection<ItemToBuy>? ItemsToBuy { get; set; } = new HashSet<ItemToBuy>();
        public ICollection<Budget>? Budgets { get; set; } = new HashSet<Budget>();
    }


}

