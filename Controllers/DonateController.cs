using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using MTGBotWebsite.Helpers;
using MTGOLibrary.Models;

namespace MTGBotWebsite.Controllers
{
    public class DonateController : Controller
    {
        private readonly MainDbContext db = new MainDbContext();
        //
        // GET: /Donate/

        public ActionResult Index()
        {
            return View(db.Donations.OrderByDescending(d => d.Amount).Take(20).ToList());
        }

        public ActionResult Success()
        {
            return View();
        }

        public void Notify()
        {
            //Post back to either sandbox or live
            //string strSandbox = "https://www.sandbox.paypal.com/cgi-bin/webscr";
            const string strLive = "https://www.paypal.com/cgi-bin/webscr";
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(strLive);

            //Set values for the request back
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            byte[] param = Request.BinaryRead(HttpContext.Request.ContentLength);
            string strRequest = Encoding.ASCII.GetString(param);
            var req = HttpUtility.ParseQueryString(strRequest);
            strRequest += "&cmd=_notify-validate";
            httpWebRequest.ContentLength = strRequest.Length;

            //for proxy
            //WebProxy proxy = new WebProxy(new Uri("http://url:port#"));
            //req.Proxy = proxy;

            //Send the request to PayPal and get the response
            StreamWriter streamOut = new StreamWriter(httpWebRequest.GetRequestStream(), Encoding.ASCII);
            streamOut.Write(strRequest);
            streamOut.Close();
            StreamReader streamIn = new StreamReader(httpWebRequest.GetResponse().GetResponseStream());
            string strResponse = streamIn.ReadToEnd();
            streamIn.Close();

            switch (strResponse)
            {
                case "VERIFIED":
                    {
                        var body = new StringBuilder();

                        body.AppendLine("Verified PayPal donation from website.");
                        body.AppendLine();
                        body.AppendLine();

                        foreach (string r in req)
                            body.AppendLine(string.Format("{0}: {1}", r, req[r]));

                        Email.SendMessage("MTGBot donation IPN", body.ToString());

                        if (req["receiver_email"] == "danbopes@gmail.com" && req["payment_status"] == "Completed")
                        {
                            db.Donations.Add(new Donation
                                {
                                    Email = req["payer_email"],
                                    Username = req["custom"],
                                    DateTime = DateTime.Now,
                                    Amount = Convert.ToDouble(req["mc_gross"]),
                                    TxnId = req["txn_id"]
                                });
                            db.SaveChanges();
                        }
                        return;
                    }
                case "INVALID":
                    {
                        var body = new StringBuilder();

                        body.AppendLine("Invalid PayPal donation from website.");
                        body.AppendLine();
                        body.AppendLine();

                        foreach (string r in req)
                            body.AppendLine(string.Format("{0}: {1}", r, req[r]));

                        Email.SendMessage("MTGBot donation", body.ToString());
                        return;
                    }
            }

            throw new HttpResponseException(HttpStatusCode.InternalServerError);
        }
    }
}
