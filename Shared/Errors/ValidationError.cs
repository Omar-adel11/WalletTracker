using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Errors
{
    public class ValidationError
    {
        public string Field { get; set; }
        public IEnumerable<string> Error { get; set; }

    }
}
