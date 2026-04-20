using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.DTOs.Auth
{
    public class UserLoginDTO
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(6)]  
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
