using System.Text;
using System.Web.Mvc;
using MTGO.Web.Helpers;
using MTGO.Web.Infastructure;
using MTGO.Web.Models;

namespace MTGO.Web.Controllers
{
    public class ContactController : Controller
    {
        //
        // GET: /Contact/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(ContactModel model)
        {
            if (ModelState.IsValid)
            {
                if ( SendMail(model) )
                    return View("Success");
            }
            return View();
        }

        [TwitchAuthorize]
        public ActionResult Bug()
        {
            return View();
        }

        [HttpPost]
        [TwitchAuthorize]
        public ActionResult Bug(BugReportModel model)
        {
            if (ModelState.IsValid)
            {
                if (SendMail(model))
                    return View("Success");
            }

            return View();
        }

        private bool SendMail(BugReportModel model)
        {
            var body = new StringBuilder();

            body.AppendLine("You recieved an bug report email via the mtgbot.tv website: ").AppendLine();

            body.AppendFormat("Email: {0}", model.Email).AppendLine();
            body.AppendFormat("Twitch Name: {0}", Session["user_name"]).AppendLine();

            if (Request.ServerVariables["HTTP_CF_CONNECTING_IP"] != null)
                body.AppendFormat("IP Address (From Cloudflare): {0}", Request.ServerVariables["HTTP_CF_CONNECTING_IP"]).AppendLine();
            else
                body.AppendFormat("IP Address: {0}", Request.UserHostAddress).AppendLine();

            if (Request.ServerVariables["X_FORWARDED_FOR"] != null)
                body.AppendFormat("Forwarded For: {0}", Request.ServerVariables["X_FORWARDED_FOR"]).AppendLine();

            body.AppendFormat("User Agent: {0}", Request.UserAgent).AppendLine().AppendLine().AppendLine();

            body.AppendLine("Bug:").AppendLine();
            body.AppendLine(model.Problem).AppendLine().AppendLine();
            body.AppendLine("Steps to reproduce:").AppendLine();
            body.AppendLine(model.Reproduce);

            return Email.SendMessage("Bug Report from website", body.ToString(), model.Email);
        }

        private bool SendMail(ContactModel model)
        {
            var body = new StringBuilder();

            body.AppendLine("You recieved an email via the mtgbot.tv website: ");
            body.AppendLine();

            body.AppendFormat("Email: {0}", model.Email).AppendLine();

            if (!string.IsNullOrWhiteSpace(model.Subject))
                body.AppendFormat("Subject: {0}", model.Subject).AppendLine();

            if (!string.IsNullOrWhiteSpace(model.StreamName))
                body.AppendFormat("Stream Name: {0}", model.StreamName).AppendLine();

            if (Request.ServerVariables["HTTP_CF_CONNECTING_IP"] != null)
                body.AppendFormat("IP Address (From Cloudflare): {0}", Request.ServerVariables["HTTP_CF_CONNECTING_IP"]).AppendLine();
            else
                body.AppendFormat("IP Address: {0}", Request.UserHostAddress).AppendLine();

            if (Request.ServerVariables["X_FORWARDED_FOR"] != null)
                body.AppendFormat("Forwarded For: {0}", Request.ServerVariables["X_FORWARDED_FOR"]).AppendLine();

            body.AppendFormat("User Agent: {0}", Request.UserAgent).AppendLine().AppendLine().AppendLine();

            body.AppendLine(model.Message);

            return Email.SendMessage("Email from website", body.ToString(), model.Email);
        }
    }
}
