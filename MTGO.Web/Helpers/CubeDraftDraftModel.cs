using System.Collections.Generic;
using MTGO.Common.Entities;
using MTGO.Common.Entities.CubeDraft;
using MTGO.Common.Entities.CubeDrafting;

namespace MTGO.Web.Helpers
{
    public class CubeDraftDraftModel
    {
        public int PlayerId { get; set; }
        public CubeDraft CubeDraft { get; set; }
        public ICollection<Card> Deck { get; set; }
    }
}