using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;

namespace Samba.Services
{
    public static class EMailService
    {
        public static void SendEmail(string smtpServerAddress, string smtpUser, string smtpPassword, int smtpPort, string toEmailAddress, string fromEmailAddress, string subject, string body, string fileName)
        {
            var mail = new MailMessage();
            var smtpServer = new SmtpClient(smtpServerAddress);

            mail.From = new MailAddress(fromEmailAddress);
            mail.To.Add(toEmailAddress);
            mail.Subject = subject;
            mail.Body = body;

            if (!string.IsNullOrEmpty(fileName))
                mail.Attachments.Add(new Attachment(fileName));
            smtpServer.Port = smtpPort;
            smtpServer.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPassword);
            smtpServer.EnableSsl = true;
            smtpServer.Send(mail);
        }

        public static void SendEMailAsync(string smtpServerAddress, string smtpUser, string smtpPassword, int smtpPort, string toEmailAddress, string fromEmailAddress, string subject, string body, string fileName)
        {
            var thread = new Thread(() => SendEmail(smtpServerAddress, smtpUser, smtpPassword, smtpPort, toEmailAddress, fromEmailAddress, subject, body, fileName));
            thread.Start();
        }
    }
}
