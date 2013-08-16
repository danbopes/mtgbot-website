using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MTGBotWebsite.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/

        public ActionResult Index()
        {
            return null;
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            var c = new HttpCookie("twitch_auth") {Expires = DateTime.Now.AddDays(-1)};
            Response.Cookies.Add(c);
            return RedirectToAction("Index", "Home");
        }

    }
}
