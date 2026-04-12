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

    public sealed class CurrencyMismatchException(string Operator) : MoneyInvalidOperationException($"Cannot perform {Operator} on amounts with different currencies.")
    {
        
    }

    public sealed class NotEnoughBalanceException() : MoneyInvalidOperationException("Not enough balance to perform the operation.")
    {

    }
}
