using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using MTGOLibrary.Models;

namespace MTGBotWebsite.Services
{
    public class DraftService
    {
        private readonly MainDbContext _db;

        public DraftService(MainDbContext db)
        {
            _db = db;
        }

        public void EnsureBroadcaster(CubeDraft draft, User user)
        {
            if ( draft == null || user == null || draft.Broadcaster.Name.ToLower() != user.TwitchUsername.ToLower() )
                throw new InvalidOperationException("You are not the broadcaster of this draft.");
        }

        public void EndTournament(CubeDraft draft, User user)
        {
            if ( draft == null )
                throw new InvalidOperationException("Draft not found.");

            EnsureBroadcaster(draft, user);

            draft.Status = CubeDraftStatus.ProductHandIn;
            _db.Entry(draft).State = EntityState.Modified;
            _db.SaveChanges();
        }
    }
}