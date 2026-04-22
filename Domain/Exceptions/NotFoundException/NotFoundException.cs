using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions.NullReferenceException
{
    public abstract class NotFoundException(string message) : Exception(message) { }
    public sealed class EntityNotFoundException(string entity) :
        NotFoundException($"{entity} was not found.")
    { }
}
    

  
