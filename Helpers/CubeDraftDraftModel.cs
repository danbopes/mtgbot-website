using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using MTGOLibrary.Models;

namespace MTGBotWebsite.Helpers
{
    public class CubeDraftDraftModel
    {
        public int PlayerId { get; set; }
        public CubeDraft CubeDraft { get; set; }
        public ICollection<Card> Deck { get; set; }
    }
}