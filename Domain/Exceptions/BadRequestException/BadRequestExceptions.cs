using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Exceptions.AuthExceptions;

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
    public sealed class EmailExistException() : BadRequestException("Email is already Exist")
    {
    }
    public sealed class UserNameExistException() : BadRequestException("UserName is already Exist")
    {
    }
}
