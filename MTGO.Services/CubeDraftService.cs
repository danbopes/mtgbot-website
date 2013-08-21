using System.Linq;
using MTGO.Database.Models;
using MTGO.Database.Models.CubeDrafting;
using NHibernate;
using NHibernate.Linq;

namespace MTGO.Services
{
    public class CubeDraftService
    {
        private readonly ISession _session;

        public CubeDraftService(ISession session)
        {
            _session = session;
        }

        public bool IsUserConfirmedCubeDraftPlayer(int cubeDraftId, int userId)
        {
            return _session.Query<CubeDraftPlayer>()
                    .Any(player => player.CubeDraft.Id == cubeDraftId && player.MtgoLink.User.Id == userId && player.Confirmed);
        }

        public IQueryable<CubeDraft> GetAll()
        {
            return _session.Query<CubeDraft>();
        }
    }
}
