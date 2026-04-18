using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions.NullReferenceException
{
    public abstract class NullException(string message) : Exception(message)
    {
    }

    public sealed class CategoryNullException(int id) : NullException($"Category with id : {id} is not found.")
    {
    }

    public sealed class BudgetNullException(int id) : NullException($"Budget with id : {id} is not found.")
    {
    }
    public sealed class BudgetsNullException(int UserId) : NullException($"Budgets with Userid : {UserId} is not found.")
    {
    }
    public sealed class UserNullException(int UserId) : NullException($"User with Id : {UserId} is not found.")
    {
    }
    
    public sealed class ItemNullException(int Id) : NullException($"Item with Id : {Id} is not found.")
    {
    }
    public sealed class ItemsNullException(int UserId) : NullException($"Items with Userid : {UserId} is not found.")
    {
    }
    public sealed class WalletNullException(int WalletId) : NullException($"wallet with Id : {WalletId} is not found.")
    {
    }
    public sealed class WalletsNullException(int UserId) : NullException($"Wallets with Userid : {UserId} is not found.")
    {
    }
    public sealed class TransactionNullException(int Id) : NullException($"Transcation with Userid : {Id} is not found.")
    {
    }
    public sealed class TransactionsNullException(int WalletId) : NullException($"Transactions with WalletId : {WalletId} is not found.")
    {
    }
    public sealed class InstallmentNullException(int Id) : NullException($"Transcation with Userid : {Id} is not found.")
    {
    }
    public sealed class InstallmetnsNullException(int UserId) : NullException($"Transactions with UserId : {UserId} is not found.")
    {
    }
    
}
