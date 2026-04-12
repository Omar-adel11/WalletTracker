using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Contracts;

namespace Domain.Entities
{
    public abstract class BaseEntity : IAduitable
    {
        public int id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set ; }
    }
}
