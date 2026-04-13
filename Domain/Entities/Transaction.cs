using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Contracts;
using Domain.Entities.Struct;

namespace Domain.Entities
{
    public class Transaction : BaseEntity , ISoftDeletable
    {
        public Money Amount { get; set; }
        public string? Description { get; set; } 
        public DateTimeOffset Date {  get; set; }
        public bool IsDeleted { get; set; }

        //Navigation properties
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? CategoryId { get; set; }
        public Category? Category { get; set; } = null!;

        public int? InstallmentsId { get; set; }
        public Installments? Installment {  get; set; } 

    }
}
