using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions.MoneyInvalidOperationException
{
    public abstract class MoneyInvalidOperationException(string message) : Exception(message)
    {
    }

    public sealed class CurrencyMismatchException() : MoneyInvalidOperationException($"Cannot perform Operations on amounts with different currencies.")
    {
        
    }
    public sealed class InvalidSourceException(string source) : MoneyInvalidOperationException($"There is no source named {source}")
    {

    }

    public sealed class NotEnoughBalanceException() : MoneyInvalidOperationException("Not enough balance to perform the operation.")
    {

    }

    public sealed class ItemToBuyBalanceException() : MoneyInvalidOperationException("Item is not fully funded yet.")
    {

    }
    public sealed class AllInstallmentsPaidException(int id) : MoneyInvalidOperationException("All Installments Paid .")
    {

    }
}
