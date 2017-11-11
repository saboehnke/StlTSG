using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace StlTSG
{
    public class MailHandler
    {
        private string institution;
        private string email;

        public MailHandler(string institution, string email)
        {
            this.institution = institution;
            this.email = email;
        }

        public void Send(string subject, string body)
        {
            MailAddress from = new MailAddress(ConfigurationManager.AppSettings["CDARNoReply"], ConfigurationManager.AppSettings["CDARTitle"]);
            MailAddress to = null;

            if (institution != null)
                to = new MailAddress(email, institution);
            else to = new MailAddress(email);

            MailMessage message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = body;
            message.Priority = MailPriority.High;
            message.IsBodyHtml = true;
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtpout.secureserver.net";
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.EnableSsl = false;
            smtp.Port = 80;
            smtp.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["CDARNoReply"], ConfigurationManager.AppSettings["CDARPassword"]);
            smtp.Send(message);
        }
    }
}