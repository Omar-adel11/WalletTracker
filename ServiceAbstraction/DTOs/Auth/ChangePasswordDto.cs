using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.DTOs.Auth
{
    public class ChangePasswordDto
    {
        public string oldPassword { get; set; } = string.Empty;
        public string newPassword { get; set; } = string.Empty;
    }
}
