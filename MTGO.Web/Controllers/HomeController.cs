using System;
using System.Web;
using System.Web.Mvc;
using MTGO.Web.Infastructure;

namespace MTGO.Web.Controllers
{
    [HandleError]
    public class HomeController : Controller
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [TwitchAuthorize]
        public ActionResult Test()
        {
            log.Debug("Test();");
            var userId = User.GetUserId();

            return View();
        }

        [HandleError]
        public void RedirectTwitch()
        {
            if (Request.QueryString["code"] == null)
            {
                throw new Exception("Unable to verify twitch account. Linking with twitch is required to use this feature.");
            }

            Response.Cookies.Add(new HttpCookie("twitch_auth", Request.QueryString["code"]));

            object finalRoute = new {controller = "Home", action = "Index"};
            try
            {
                var route = Request.QueryString["state"].Split('/');


                if (route.Length > 2)
                    finalRoute = new { controller = route[0], action = route[1], id = route[2] };
                else
                    finalRoute = new { controller = route[0], action = route[1] };
            }
            catch { }

            Response.RedirectToRoute(finalRoute);

            Response.End();
        }
    }
}
