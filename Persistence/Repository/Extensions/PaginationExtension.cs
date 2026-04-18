using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repository.Extensions
{
    public static class PaginationExtension
    {
        public static IEnumerable<T> whereWithPaginate<T>(this IEnumerable<T> source, Func<T, bool> predicate, int pageNumber, int pageSize)
        {
            if(source == null) throw new ArgumentNullException(nameof(source));
            if(predicate == null) throw new ArgumentNullException(nameof(predicate));

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var result = source.Where(predicate);
            return result.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }
    }
}

