using System;

namespace MTGO.Database.Models
{
    public class Ban
    {
        public virtual int Id { get; protected set; }
        public virtual string IpAddress { get; set; }
        public virtual BanType? BanType { get; set; }
        public virtual DateTime Expires { get; set; }
        public virtual string Reason { get; set; }

        public virtual User User { get; set; }
    }
}
