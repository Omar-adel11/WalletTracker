using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Helper.Cache
{
    public class CacheSettings
    {
        public int DefaultExpirationMinutes { get; set; } = 5;
        public int CategoryExpirationHours { get; set; } = 1;
        public int WalletExpirationMinutes { get; set; } = 2;
        public int DashboardExpirationMinutes { get; set; } = 5;
    }
}
