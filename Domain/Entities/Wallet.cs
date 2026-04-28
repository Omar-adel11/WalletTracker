using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Enum;
using Domain.Entities.Struct;
using Domain.Exceptions.MoneyInvalidOperationException;
using Microsoft.EntityFrameworkCore.Infrastructure;

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

       
        public void ApplyTransaction(TransactionType type, MoneySource source, decimal amount,int? categoryId = null,string? description = null,bool isUpdated = false)
        {
            if (type == TransactionType.Income)
            {
                if (source == MoneySource.Cash) Cash += amount;
                else if(source == MoneySource.Credit) Credit += amount;
                else Pended += amount;
            }
            else 
            {
                if (source == MoneySource.Cash)
                {
                    if (Cash < amount) throw new NotEnoughBalanceException();
                    Cash -= amount;
                }
                else if(source == MoneySource.Credit)
                {
                    if (Credit < amount) throw new NotEnoughBalanceException();
                    Credit -= amount;
                }
                else
                {
                    if(Pended < amount) throw new NotEnoughBalanceException();
                    Pended -= amount;
                }
            }
            if (!isUpdated)
            {
                var transaction = new Transaction
                {
                    WalletId = this.id,
                    UserId = UserId,
                    Amount = new Money { Amount = amount, Currency = this.Currency },
                    Description = description ?? $"{type} of {amount} {this.Currency} from wallet {this.id}",
                    Type = type,
                    CreatedAt = DateTimeOffset.UtcNow,
                    MoneySource = source,
                    Date = DateTimeOffset.UtcNow,
                    CategoryId = categoryId
                };
                this.Transactions.Add(transaction);
            }
        }

        public void UndoTransaction(TransactionType type, MoneySource source, decimal amount)
        {

            if (type == TransactionType.Income)
            {
                if (source == MoneySource.Cash) Cash -= amount;
                else Credit -= amount;
            }
            else
            {
                if (source == MoneySource.Cash) Cash += amount;
                else Credit += amount;
            }
        }

        //Navegation properties
        public ICollection<Transaction>? Transactions { get; set; } = new HashSet<Transaction>();
        public ICollection<ItemToBuy>? ItemsToBuy { get; set; } = new HashSet<ItemToBuy>();
        public ICollection<Budget>? Budgets { get; set; } = new HashSet<Budget>();
    }


}

