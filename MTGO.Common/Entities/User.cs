using System;
using System.Collections.Generic;
using MTGO.Common.Entities.Mtgo;

namespace MTGO.Common.Entities
{
    public class User
    {
        public User()
        {
            Bans = new List<Ban>();
            Drafts = new List<Draft>();
            MtgoLinks = new List<MtgoLink>();
        }

        public virtual int Id { get; protected set; }
        public virtual bool Admin { get; protected set; }
        public virtual string Username { get; protected set; }
        public virtual string SignupIpAddress { get; protected set; }
        public virtual DateTime Created { get; protected set; }

        public virtual Broadcaster Broadcaster { get; protected set; }
        public virtual IList<Ban> Bans { get; protected set; }
        public virtual IList<Draft> Drafts { get; protected set; }
        public virtual IList<MtgoLink> MtgoLinks { get; protected set; }
    }
}
