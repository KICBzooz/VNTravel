using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MailKit;

namespace VNTravel.Models
{
    public class SendEmail
    {
        public int CountEmail() 
        {
            var client = new ImapClient();
            client.Connect("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
            client.Authenticate("kicbzooz1432@gmail.com", "hgzjdhglyhuwrbze");
            client.Inbox.Open(FolderAccess.ReadOnly);
            var uids = client.Inbox.Search(SearchQuery.New);
            uids = client.Inbox.Search(SearchQuery.NotSeen);
            client.Disconnect(true);
            return uids.Count;
        }
        public Boolean SendAccount(string accompanyemail, string accompanyname)
        {
            //"kicbzooz1432@gmail.com", "hgzjdhglyhuwrbze"
            var fromAddress = new MailAddress("kicbzooz1432@gmail.com", "TravelWeb");
            var toAddress = new MailAddress(accompanyemail,"");
            const string fromPassword = "hgzjdhglyhuwrbze";
            const string subject = "Notification!";
            const string body = "   Your Account \n Username: " + "Your Gmail" + " \n Password: " + "Pass123";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                Timeout = 20000
            };
            try {
                using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body
                    }
                )
                {
                    smtp.Send(message);
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
            
        }
    }
}