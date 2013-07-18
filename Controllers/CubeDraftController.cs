using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
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
            return View(db.CubeDrafts.Where(d => d.Status != CubeDraftStatus.Init).OrderByDescending(d => d.Id).Take(10).ToList());
        }

        public ActionResult Create()
        {
            Authorization.Authorize();

            var user = (string)Session["user_name"];
            var broadcaster = db.Broadcasters.FirstOrDefault(b => b.Name == user);

            if ( broadcaster == null )
                throw new Exception("Sorry, this feature is only for broadcasters on twitch.tv (With a valid mtgbot.tv client application).");

            var draft =
                db.CubeDrafts.FirstOrDefault(
                    c => c.Status == CubeDraftStatus.PreStart && c.BroadcasterId == broadcaster.Id);

            //if ( db.CubeDrafts.Any(c => c.Status != CubeDraftStatus.Exception && c.Status != CubeDraftStatus.Completed) )
            //    throw new Exception("Sorry, only one draft may be running at one time. This restriction may be lifted in the future.");

            return View(draft);
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

            var user = (string) Session["user_name"];

            var cubeDraft = db.CubeDrafts.Find(id);

            if (cubeDraft == null)
                throw new Exception(String.Format("Unable to find CubeDraft with id='{0}'", id));

            if (cubeDraft.Status == CubeDraftStatus.Init || cubeDraft.Status == CubeDraftStatus.PreStart)
                return RedirectToAction("View", new { Id = id });

            var player = cubeDraft.CubeDraftPlayers.SingleOrDefault(p => p.MTGOUsername.TwitchUsername == user);

            if (player == null)
                return RedirectToAction("View", new {Id = id});

            var cardIds = cubeDraft.CubeDraftPicks.Where(p => p.PlayerId == player.Id && p.PickId != null).Select(p => p.PickId).ToArray();
            var cardObjects = db.Cards.Where(c => cardIds.Contains(c.Id)).ToArray();
            var cards = cardIds.Select(c => cardObjects.Single(co => co.Id == c)).ToArray();

            return View(new CubeDraftDraftModel
            {
                CubeDraft = cubeDraft,
                PlayerId = player.Id,
                Deck = cards
            });
        }

        public ActionResult Deck(int id, string sideboardIds)
        {
            Authorization.Authorize();

            var cubeDraft = db.CubeDrafts.Find(id);

            if (cubeDraft == null)
                throw new Exception(String.Format("Unable to find CubeDraft with id='{0}'", id));

            if (cubeDraft.Status != CubeDraftStatus.Drafting)
                return RedirectToAction("View", id);

            var player = db.CubeDraftPlayers.SingleOrDefault(p => p.MTGOUsername.TwitchUsername == (string)Session["user_name"]);

            if (player == null)
                return RedirectToAction("View", id);

            var cardIds = cubeDraft.CubeDraftPicks.Where(p => p.PlayerId == player.Id).Select(p => p.PickId).ToArray();
            var cardObjects = db.Cards.Where(c => cardIds.Contains(c.Id)).ToArray();
            var cards = cardIds.Select(c => cardObjects.Single(co => co.Id == c)).OrderBy(c => c.Name).ToArray();

            return File(Encoding.ASCII.GetBytes(String.Join("\n", cards.Select(x => "1 " + x.Name))), "text/plain", Utils.MakeValidFileName(cubeDraft.Name) + ".txt");
        }
    }
}
