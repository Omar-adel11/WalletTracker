using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Helper
{
    public class PaymobSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string WebhookHmacSecret { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://accept.paymob.com";
        public int PremiumAmountCents { get; set; } = 69900;   // 699 EGP
        public string Currency { get; set; } = "EGP";
        public string NotificationUrl { get; set; } = string.Empty; // your webhook URL
        public string RedirectionUrl { get; set; } = string.Empty;  // frontend success page
    }
}
