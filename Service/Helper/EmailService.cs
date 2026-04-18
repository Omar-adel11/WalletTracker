using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;


using ServiceAbstraction.Helper.Email;

namespace Service.Helper
{
    public class EmailService(IOptions<EmailSettings> _settings) : ServiceAbstraction.Helper.Email.IEmailService
    {
        public async Task SendEmailAsync(Email email)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(_settings.Value.SenderName, _settings.Value.SenderEmail));
            message.To.Add(MailboxAddress.Parse(email.To));

            message.Subject = email.Subject;

            message.Body = new TextPart("html")
            {
                Text = email.Body
            };

            try
            {
                using var smtp = new SmtpClient();
                
                await smtp.ConnectAsync(
                    _settings.Value.SmtpServer,
                    _settings.Value.SmtpPort,
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    _settings.Value.SenderEmail,
                    _settings.Value.Password
                );

                await smtp.SendAsync(message);

                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Email sending failed. Please try again later.",
                    ex
                );
            }

        }
    }
}
