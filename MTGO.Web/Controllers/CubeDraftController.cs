using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using MTGO.Common.Entities.CubeDrafting;
using MTGO.Common.Enums;
using MTGO.Services;
using MTGO.Web.Filters;
using MTGO.Web.Helpers;
using MTGO.Web.Infastructure;
using NHibernate;
using NHibernate.Linq;

namespace MTGO.Web.Controllers
{
    public class CubeDraftController : Controller
    {
        private readonly CubeDraftService _cubeDraftService;

        public CubeDraftController(CubeDraftService cubeDraftService)
        {
            _cubeDraftService = cubeDraftService;
        }

        [TransactionFilter]
        public ActionResult Index()
        {
            var drafts = _cubeDraftService.GetAll()
                    .Where(draft => draft.Status != CubeDraftStatus.Init)
                    .OrderByDescending(draft => draft.Id)
                    .Take(10)
                    .ToList();

            return View(drafts);
        }

        [TwitchAuthorize]
        public ActionResult Create()
        {
            var userId = User.GetUserId();

            var user = db.Users.Find(userId);

            var broadcaster = db.Broadcasters.FirstOrDefault(b => b.Name == user.TwitchUsername);

            if ( broadcaster == null )
                throw new Exception("Sorry, this feature is only for broadcasters on twitch.tv (With a valid mtgbot.tv client application).");

            var draft =
                db.CubeDrafts.FirstOrDefault(
                    c => c.Status == CubeDraftStatus.PreStart && c.BroadcasterId == broadcaster.Id);

            //if ( db.CubeDrafts.Any(c => c.Status != CubeDraftStatus.Exception && c.Status != CubeDraftStatus.Completed) )
            //    throw new Exception("Sorry, only one draft may be running at one time. This restriction may be lifted in the future.");

            return View(draft);
        }

        [TwitchAuthorize]
        public ActionResult View(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");

            var cubeDraft = db.CubeDrafts
                .Include("Broadcaster")
                .Include("CubeDraftPlayers.MtgoLink")
                .SingleOrDefault(d => d.Id == id);

            if ( cubeDraft == null )
                throw new Exception(String.Format("Unable to find CubeDraft with id='{0}'", id));
            
            if (cubeDraft.Status == CubeDraftStatus.Drafting)
            {
                var player = cubeDraft.CubeDraftPlayers.FirstOrDefault(p => p.MtgoLink.UserId == User.GetUserId());

                if (player != null && player.Confirmed)
                    return RedirectToAction("Draft", new {Id = id});
            }

            var user = db.Users.Find(User.GetUserId());

            ViewBag.IsBroadcaster = user.TwitchUsername.ToLower() == cubeDraft.Broadcaster.Name.ToLower();

            return View(cubeDraft);
        }

        [TwitchAuthorize]
        public ActionResult Draft(int? id)
        {
            if (id == null)
                return RedirectToAction("Index");

            var userId = User.GetUserId();

            var cubeDraft = db.CubeDrafts
                .Include("CubeDraftPlayers.MtgoLink.User")
                .Include("CubeDraftPicks")
                .SingleOrDefault(d => d.Id == id);

            if (cubeDraft == null)
                throw new Exception(String.Format("Unable to find CubeDraft with id='{0}'", id));

            if (cubeDraft.Status == CubeDraftStatus.Init || cubeDraft.Status == CubeDraftStatus.PreStart)
                return RedirectToAction("View", new { Id = id });

            var player = cubeDraft.CubeDraftPlayers.SingleOrDefault(p => p.MtgoLink.UserId == userId);

            if (player == null)
                return RedirectToAction("View", new {Id = id});

            var cardIds = cubeDraft.CubeDraftPicks.Where(p => p.PlayerId == player.Id && p.PickId != null).Select(p => p.PickId).ToArray();
            var cardObjects = db.Cards.Include("CardSet").Where(c => cardIds.Contains(c.Id)).ToArray();
            var cards = cardIds.Select(c => cardObjects.Single(co => co.Id == c)).ToArray();

            return View(new CubeDraftDraftModel
            {
                CubeDraft = cubeDraft,
                PlayerId = player.Id,
                Deck = cards
            });
        }

        [TwitchAuthorize]
        public ActionResult Deck(int id)
        {

            var sideboardString = Request["sideboard"];

            var userId = User.GetUserId();

            var sideboardIds = new int[0];
            try
            {
                sideboardIds = sideboardString.Split(',').Select(i => Convert.ToInt32(i)).ToArray();
            }
            catch (FormatException)
            {
            }
            catch(NullReferenceException)
            {}

            var cubeDraft = db.CubeDrafts
                .Include("CubeDraftPlayers.MtgoLink")
                .Include("CubeDraftPicks")
                .SingleOrDefault(d => d.Id == id);

            if (cubeDraft == null)
                throw new Exception(String.Format("Unable to find CubeDraft with id='{0}'", id));

            var player = cubeDraft.CubeDraftPlayers.SingleOrDefault(p => p.MtgoLink.UserId == userId);

            if (player == null)
                return RedirectToAction("View", id);

            var cardIds = cubeDraft.CubeDraftPicks.Where(p => p.PlayerId == player.Id).Select(p => p.PickId).ToArray();
            var cardObjects = db.Cards.Include("CardSet").Where(c => cardIds.Contains(c.Id)).ToArray();
            var cards = cardIds.Select(c => cardObjects.Single(co => co.Id == c)).OrderBy(c => c.Name).ToArray();
            /*<?xml version="1.0" encoding="utf-8"?>
<Deck xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <NetDeckID>0</NetDeckID>
  <PreconstructedDeckID>0</PreconstructedDeckID>
  <Cards CatID="30088" Quantity="1" Sideboard="false" Name="" Row="0" Col="0" />
  <Cards CatID="38163" Quantity="1" Sideboard="false" Name="" Row="0" Col="2" />
  <Cards CatID="45350" Quantity="2" Sideboard="false" Name="" Row="0" Col="3" />
  <Cards CatID="48728" Quantity="1" Sideboard="false" Name="" Row="0" Col="1" />
  <Cards CatID="35134" Quantity="1" Sideboard="true" Name="" Row="0" Col="0" />
  <Cards CatID="39477" Quantity="1" Sideboard="true" Name="" Row="0" Col="0" />
  <Cards CatID="39788" Quantity="1" Sideboard="true" Name="" Row="0" Col="0" />
</Deck>*/
            using (var sw = new StringWriter())
            {
                using (XmlWriter w = XmlWriter.Create(sw))
                {
                    w.WriteStartDocument();
                    w.WriteStartElement("Deck");
                    w.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                    w.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
                    w.WriteElementString("NetDeckID", "0");
                    w.WriteElementString("PreconstructedDeckID", "0");

                    foreach (var card in cards)
                    {
                        w.WriteStartElement("Cards");
                        w.WriteAttributeString("CatID", card.Id.ToString());
                        w.WriteAttributeString("Quantity", "1");
                        w.WriteAttributeString("Sideboard", sideboardIds.Contains(card.Id).ToString().ToLower());
                        w.WriteAttributeString("Name", String.Empty);
                        w.WriteAttributeString("Row", "0");
                        w.WriteAttributeString("Col", "0");
                        w.WriteEndElement();
                    }
                    w.WriteEndElement();
                }

                return File(Encoding.UTF8.GetBytes(sw.ToString().Replace("utf-16", "utf-8")), "text/plain", Utils.MakeValidFileName(cubeDraft.Name) + ".dek");
            }

            //var deck = String.Join("\r\n", cards.Where(c => !sideboardIds.Contains(c.Id)).Select(x => "1 " + x.Name.Replace("Æ", "AE")));
            //var sideboard = String.Join("\r\n", cards.Where(c => sideboardIds.Contains(c.Id)).Select(x => "1 " + x.Name));
        }
    }
}
