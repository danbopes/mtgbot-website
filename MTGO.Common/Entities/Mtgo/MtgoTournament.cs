using System;
using System.Collections.Generic;

namespace MTGO.Common.Entities.Mtgo
{
    public class MtgoTournament
    {
        public virtual int Id { get; set; }
        public virtual Guid Token { get; set; }
        public virtual string Description { get; set; }
        public virtual DateTime StartDate { get; set; }

        public virtual IList<MtgoMatch> Matches { get; protected set; }

        public MtgoTournament()
        {
            Matches = new List<MtgoMatch>();
        }
    }
}
