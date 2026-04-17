using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.DTOs.WalletsDtos
{
    public class WalletDTO
    {
        public string Currency { get; set; } = "EGP";
        public decimal TotalBalance => Cash + Credit + Pended;
        public decimal Cash { get; set; }
        public decimal Credit { get; set; }
        public decimal Pended { get; set; }
    }
}
