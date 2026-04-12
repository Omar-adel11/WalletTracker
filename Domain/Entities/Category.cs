using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        //Navigation properties
        public int? UserId { get; set; }
        public User? User { get; set; }

        public ICollection<Transaction>? Transactions { get; set; } = new HashSet<Transaction>();
        public ICollection<Budget>? Budgets { get; set; } = new HashSet<Budget>();
        public ICollection<Installments>? Installments { get; set; } = new HashSet<Installments>();

    }
}
