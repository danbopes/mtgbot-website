using System.Web.Mvc;

namespace MTGBotWebsite.Controllers
{
    [OutputCache(Duration = 3600)]
    public class CommandsController : Controller
    {
        //
        // GET: /Command/
        public ActionResult Index()
        {
            return View();
        }

    }
}
