using System;
using System.Linq;
using System.Web.Mvc;
using MTGBotWebsite.Helpers;
using MTGBotWebsite.Models;

namespace MTGBotWebsite.Controllers
{
    public class DraftViewerController : Controller
    {
        //
        // GET: /DraftViewer/
        readonly MainDbContext db = new MainDbContext();

        public ActionResult Index()
        {
            return View(db.Drafts.OrderByDescending(d => d.Id).Take(10).ToList());
        }

        public ActionResult Draft(int id = 0)
        {
            Authorization.Authorize();

            var draft = db.Drafts.Find(id);

            if ( draft == null )
                throw new Exception(string.Format("Draft not found with id='{0}'", id));

            return View(draft);
        }
    }
}
