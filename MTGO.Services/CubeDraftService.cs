using System;
using System.Linq;
using MTGO.Common.Entities.CubeDrafting;
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

        public IQueryable<CubeDraft> GetAll()
        {
            return _session.Query<CubeDraft>();
        }
    }
}
