using System.Web.Mvc;

namespace MTGO.Web.Controllers
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
