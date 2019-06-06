using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace FrontPipedriveIntegrationProject
{
    class EmailSender
    {
        SmtpClient smtpServer;
        public MailMessage mail;
        StringBuilder body;

        public EmailSender() {
            mail = new MailMessage();
            body = new StringBuilder("");
            smtpServer = new SmtpClient("smtp.gmail.com");

            mail.From = new MailAddress("emailsender1995@gmail.com");
            mail.To.Add("piyush@leanserver.com,keegan@leanserver.com,mike@leanserver.com,jill@leanserver.com,support@leansentry.com");

            mail.Subject = "Conversation summaries";
            mail.IsBodyHtml = true;


            smtpServer.Port = 587;
            smtpServer.Credentials = new System.Net.NetworkCredential("emailsender1995", "sendemail");
            smtpServer.EnableSsl = true;
        }

        public void SendMessage()
        {
            try
            {
                mail.Body = body.ToString();
                smtpServer.Send(mail);
                Console.WriteLine("mail Sent");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void AppendLineToEmailBody(string line)
        {
            body.Append(line).Append("<br>");
        }
    }
}