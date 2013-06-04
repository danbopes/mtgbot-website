using System.Web.Mvc;
using MTGBotWebsite.Helpers;
using MTGBotWebsite.Models;

namespace MTGBotWebsite.Controllers
{
    public class TriviaController : Controller
    {
        //
        // GET: /Trivia/

        public ActionResult Index()
        {
            Authorization.Authorize();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(TriviaModel model)
        {
            Authorization.Authorize();
            if (ModelState.IsValid)
            {
                Pipe.SendMessage("{0}|TriviaGame|{1}",
                    Session["user_name"],
                    model.Name);
            }
            return View();
        }


    }
}
