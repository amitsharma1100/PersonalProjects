using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Deepwell.Common.Helpers
{
    public class Mailer
    {
        public static void Send(string toAddress, string subject, string body, string fromAddress = "")
        {
            SmtpSection section = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");

            using (var smtpClient = new SmtpClient(section.Network.Host, section.Network.Port))
            {
                smtpClient.Credentials = new NetworkCredential(section.Network.UserName, section.Network.Password);
                smtpClient.EnableSsl = section.Network.EnableSsl;
                string from = fromAddress != "" ? fromAddress : section.From;
                using (MailMessage mailMessage = new MailMessage(from, toAddress))
                {
                    mailMessage.IsBodyHtml = true;
                    mailMessage.Body = body;
                    mailMessage.Subject = subject;
                    smtpClient.Send(mailMessage);
                };
            }
        }

        public static Task SendAsync(string toAddress, string subject, string body, string fromAddress = "")
        {
            SmtpSection section = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");

            using (var smtpClient = new SmtpClient(section.Network.Host, section.Network.Port))
            {
                smtpClient.Credentials = new NetworkCredential(section.Network.UserName, section.Network.Password);
                smtpClient.EnableSsl = section.Network.EnableSsl;
                string from = fromAddress != "" ? fromAddress : section.From;
                using (MailMessage mailMessage = new MailMessage(from, toAddress))
                {
                    mailMessage.IsBodyHtml = true;
                    mailMessage.Body = body;
                    mailMessage.Subject = subject;
                    return smtpClient.SendMailAsync(mailMessage);
                };
            }
        }
    }
}
