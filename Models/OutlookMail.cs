using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;

namespace ChinaTea.Models
{
    public class OutlookMail
    {
        string _sender = "";
        string _password = "";
        public OutlookMail(string sender, string password)
        {
            _sender = sender;
            _password = password;
        }  

  
        public void SendMail(string recipient, string subject, string message)
        {
            SmtpClient client = new SmtpClient("mail.leotang.net");
  
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            System.Net.NetworkCredential credentials = 
                new System.Net.NetworkCredential(_sender, _password);
            client.EnableSsl = true;
            client.Credentials = credentials;
  
            try
            {
                var mail = new MailMessage(_sender.Trim(), recipient.Trim());
                mail.Subject = subject;
                mail.Body = message;
                mail.IsBodyHtml = true;
                client.Send(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }

    }
}