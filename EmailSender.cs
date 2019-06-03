using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace FrontPipedriveIntegrationProject
{
    //todo CONVERT BODY TO STRING BUILDER
    class EmailSender
    {
        SmtpClient smtpServer;
        public MailMessage mail;
        string body;

        public EmailSender() {
            mail = new MailMessage();
            body = "";
            smtpServer = new SmtpClient("smtp.gmail.com");

            mail.From = new MailAddress("emailsender1995@gmail.com");
            mail.To.Add("piyush@leanserver.com");
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
                mail.Body = body;
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
            body = body + line + "<br>";
        }
    }
}