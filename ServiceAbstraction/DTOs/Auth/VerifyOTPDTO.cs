using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.DTOs.Auth
{
    public class VerifyOTPDTO
    {
        public string Email { get; set; } = string.Empty;
        public string OTP { get; set; }
    }
}
