using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Helper.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(Email email);
    }
}
