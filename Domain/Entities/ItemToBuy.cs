using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Struct;

namespace Domain.Entities
{
    public class ItemToBuy : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
            public Money Price { get; set; }
            public Money Amount { get; set; }
            public bool IsAchieved { get; set; }

        //Navigation properties
        public int UserId { get; set; }
        public User User { get; set; } = null!; 
    }
}
