using System.Collections.Generic;
using MTGO.Common.Models;

namespace MTGO.Web.Helpers
{
    public class CubeDraftDraftModel
    {
        public int PlayerId { get; set; }
        public CubeDraft CubeDraft { get; set; }
        public ICollection<Card> Deck { get; set; }
    }
}