using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Struct;

namespace Domain.Entities
{
    public class Budget : BaseEntity
    {
        public Money Limit { get; set; }
        public Money Spent { get; set; }
        public bool IsDisabled { get; set; }

        //Navigation properties
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
