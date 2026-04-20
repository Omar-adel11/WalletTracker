using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions.BadRequestException
{
    public abstract class BadRequestException(string message) : Exception(message)
    {
    }

    public sealed class RegisterationBadRequestException(IEnumerable<string> Error) : BadRequestException(string.Join(",", Error))
    {
    }

    public sealed class CurrentPasswordBadRequestException() : BadRequestException("CurrentPassword is not correct")
    {
    }
}
