using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;

namespace Samba.Services
{
    public static class EMailService
    {
        public static void SendEmail(string smtpServerAddress, string smtpUser, string smtpPassword, int smtpPort, string toEmailAddress, string fromEmailAddress, string subject, string body, string fileName, bool deleteFile)
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
            try
            {
                smtpServer.Send(mail);
            }
            catch (Exception e)
            {   
                AppServices.LogError(e);
            }
            finally
            {
                if (deleteFile && !string.IsNullOrEmpty(fileName) && File.Exists(fileName))
                    File.Delete(fileName);
            }
        }

        public static void SendEMailAsync(string smtpServerAddress, string smtpUser, string smtpPassword, int smtpPort, string toEmailAddress, string fromEmailAddress, string subject, string body, string fileName, bool deleteFile)
        {
            var thread = new Thread(() => SendEmail(smtpServerAddress, smtpUser, smtpPassword, smtpPort, toEmailAddress, fromEmailAddress, subject, body, fileName, deleteFile));
            thread.Start();
        }
    }
}
