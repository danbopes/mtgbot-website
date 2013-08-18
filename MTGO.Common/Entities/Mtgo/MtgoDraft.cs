using System;

namespace MTGO.Common.Entities.Mtgo
{
    public class MtgoDraft
    {
        public virtual int Id { get; set; }
        public virtual Guid Token { get; set; }
    }
}
