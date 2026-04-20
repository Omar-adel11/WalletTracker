using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Exceptions.NullReferenceException;

namespace Domain.Exceptions.AuthExceptions
{
    public abstract class AuthException(string message) : Exception(message)
    {
    }
    public sealed class UserNotFoundNullException() : AuthException($"User is not found.")
    {
    }
    public sealed class UnAuthorizedException(string message) : AuthException(message)
    {
    }

    public sealed class EmailExistException() : AuthException("Email is already Exist")
    {
    }
    public sealed class UserNameExistException() : AuthException("UserName is already Exist")
    {
    }
}
