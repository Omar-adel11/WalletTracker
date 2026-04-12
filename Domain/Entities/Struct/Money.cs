using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Exceptions.MoneyInvalidOperationException;

namespace Domain.Entities.Struct
{
    public readonly record struct Money(decimal Amount, string Currency)
    {
        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new CurrencyMismatchException("add");
            return new Money(a.Amount + b.Amount, a.Currency);
        }
        public static Money operator -(Money a, Money b)
        {
            if(a.Currency != b.Currency)
                throw new CurrencyMismatchException("Subtract");
            return new Money(a.Amount - b.Amount, a.Currency);
        }
        public override string ToString()
        {
            return $"{Amount} {Currency}";
        }
    }

}
