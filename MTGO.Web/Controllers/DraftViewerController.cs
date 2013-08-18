using System;
using System.Linq;
using System.Web.Mvc;
using MTGO.Web.Infastructure;

namespace MTGO.Web.Controllers
{
    public class DraftViewerController : Controller
    {
        //
        // GET: /DraftViewer/
        readonly MainDbContext db = new MainDbContext();

        public ActionResult Index()
        {
            return View(db.Drafts.Include("Broadcaster").OrderByDescending(d => d.Id).Take(10).ToList());
        }

        [TwitchAuthorize]
        public ActionResult Draft(int id = 0)
        {
            var draft = db.Drafts.Include("Broadcaster").SingleOrDefault(d => d.Id == id);

            if ( draft == null )
                throw new Exception(string.Format("Draft not found with id='{0}'", id));

            return View(draft);
        }
    }
}
