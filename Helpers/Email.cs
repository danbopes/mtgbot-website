using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace MTGBotWebsite.Helpers
{
    public class Email
    {
        public static bool SendMessage(string subject, string body, string from = null)
        {
             var ss = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    Timeout = 10000,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("danbopes", ConfigurationManager.AppSettings["EmailPassword"])
                };

            try
            {
                using (var mm = new MailMessage("danbopes@gmail.com", "danbopes+mtgbot@gmail.com", subject, body)
                {
                    BodyEncoding = Encoding.UTF8,
                    DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure
                })
                {
                    if ( from != null )
                        mm.ReplyToList.Add(from);

                    ss.Send(mm);
                }
                return true;
            }
            catch (SmtpException)
            {
                return false;
            }
        }
    }
}