using System;
using System.Linq;
using System.Web.Mvc;
using MTGBotWebsite.Helpers;
using MTGBotWebsite.Infastructure;
using MTGOLibrary.Models;

namespace MTGBotWebsite.Controllers
{
    public class TournamentCenterController : Controller
    {
        private readonly MainDbContext _db = new MainDbContext();

        public ActionResult Index()
        {
            return View(_db.Drafts.OrderByDescending(d => d.Id).Take(10).ToList());
        }

        [TwitchAuthorize]
        public ActionResult Draft(int id = 0)
        {
            var draft = _db.Drafts.Find(id);

            if (draft == null)
                throw new Exception(string.Format("Draft not found with id='{0}'", id));

            return View(draft);
        }
    }
}