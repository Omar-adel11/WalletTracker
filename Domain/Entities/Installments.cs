using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Struct;

namespace Domain.Entities
{
    public class Installments : BaseEntity
    {
        public Money Amount { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public DateTimeOffset LastDate { get; set; }
        public string? To { get; set; }
        //Navigateion properties
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? CategoryId { get; set; }
        public Category? Category { get; set; } = null!;

        public ICollection<Transaction>? transactions { get; set; } = new HashSet<Transaction>();

    }
}
