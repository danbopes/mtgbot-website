using MTGO.Database.Models.Mtgo;

namespace MTGO.Database.Models
{
    public class MtgoLink
    {
        public virtual int Id { get; protected set; }
        public virtual bool Confirmed { get; protected set; }
        public virtual string ConfirmKey { get; protected set; }

        public virtual User User { get; protected set; }
        public virtual MtgoPlayer Player { get; protected set; }
    }
}
