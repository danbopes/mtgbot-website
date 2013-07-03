using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MTGBotWebsite.CubeDrafts;
using MTGBotWebsite.Helpers;
using MTGOLibrary.Models;

namespace MTGBotWebsite.Controllers
{
    public class CubeDraftController : Controller
    {
        //
        // GET: /CubeDraft/
        readonly MainDbContext db = new MainDbContext();

        public ActionResult Index()
        {
            return View(db.CubeDrafts.OrderByDescending(d => d.Id).Take(10).ToList());
        }

        public ActionResult Create()
        {
            Authorization.Authorize();

            var user = (string)Session["user_name"];
            var broadcaster = db.Broadcasters.FirstOrDefault(b => b.Name == user);

            if ( broadcaster == null )
                throw new Exception("Sorry, this feature is only for broadcasters on twitch.tv (With a valid mtgbot.tv client application).");
            
            if ( db.CubeDrafts.Any(c => c.Status != CubeDraftStatus.Exception && c.Status != CubeDraftStatus.Completed) )
                throw new Exception("Sorry, only one draft may be running at one time. This restriction may be lifted in the future.");

            return View();
        }

        public ActionResult View(int id)
        {
            Authorization.Authorize();

            var cubeDraft = db.CubeDrafts.Find(id);

            if ( cubeDraft == null )
                throw new Exception(String.Format("Unable to find CubeDraft with id='{0}'", id));

            return View(cubeDraft);
        }

        public ActionResult Draft(int id)
        {
            Authorization.Authorize();

            var cubeDraft = db.CubeDrafts.Find(id);

            if (cubeDraft == null)
                throw new Exception(String.Format("Unable to find CubeDraft with id='{0}'", id));

            return View(cubeDraft);
        }

        public ActionResult Test()
        {
            var tournament = new SwissTournament(new []
                {
                    new Player(1, "Jim"),
                    new Player(2, "Bob"),
                    new Player(3, "Sally"),
                    new Player(4, "Joe"),
                    new Player(5, "Daniel"),
                    new Player(6, "Chris")
                });

            var parings = tournament.PairNextRound();

            string output = "";
            foreach (var pair in parings)
            {
                output += pair.Player1 + " vs. " + pair.Player2 + Environment.NewLine;
            }

            throw new Exception(output);
        }
    }
}
