using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit.Text;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;

namespace eBookSite.Utility
{
    public class EmailSender
    {
        private IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
         _configuration = configuration;
        }

        public void SendEmail(string to, string subject, string html, string from = null)
        {
            // create message
            using (var email = new MimeMessage())
            {
                email.From.Add(MailboxAddress.Parse(from ?? _configuration.GetSection("EmailSettings").GetSection("EmailFrom").Value));
                email.To.Add(MailboxAddress.Parse(to));
                email.Cc.Add(MailboxAddress.Parse(_configuration.GetSection("EmailSettings").GetSection("CC").Value));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = html };

                // send email
                using var smtp = new SmtpClient();
                smtp.Connect(_configuration.GetSection("EmailSettings").GetSection("SmtpHost").Value, Convert.ToInt32(_configuration.GetSection("EmailSettings").GetSection("SmtpPort").Value), SecureSocketOptions.StartTls);
                smtp.Authenticate(_configuration.GetSection("EmailSettings").GetSection("SmtpUser").Value, _configuration.GetSection("EmailSettings").GetSection("SmtpPass").Value);
                smtp.Send(email);
                smtp.Disconnect(true);
            }
        }
    }
}
