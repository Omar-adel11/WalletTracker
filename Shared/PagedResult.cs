using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;


namespace Shared
{
    public record PagedResult<T>(
     IReadOnlyList<T> Items,
     int TotalCount,
     int Page,
     int PageSize)
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}