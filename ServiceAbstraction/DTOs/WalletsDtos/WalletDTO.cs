using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Struct;

namespace ServiceAbstraction.DTOs.WalletsDtos
{
    public class WalletDTO
    {
        public Money TotalBalance => Cash + Credit + Pended;
        public Money Cash { get; set; }
        public Money Credit { get; set; }
        public Money Pended { get; set; }
    }
}
