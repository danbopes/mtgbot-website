using System;

namespace MTGO.Database.Models.Mtgo
{
    public class MtgoDraft
    {
        public virtual int Id { get; set; }
        public virtual Guid Token { get; set; }
    }
}
