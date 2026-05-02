using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.DTOs.Auth
{
    public class UserDTO
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PictureUrl { get; set; }
        public string Plan { get; set; } = "Free";
        public bool IsPremium { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
