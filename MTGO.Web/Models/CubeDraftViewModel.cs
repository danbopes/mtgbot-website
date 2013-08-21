using System.Collections.Generic;
using MTGO.Database.Models.CubeDrafting;

namespace MTGO.Web.Models
{
    public class CubeDraftViewModel
    {
        public int CubeDraftId { get; set; }
        public string DraftName { get; set; }
        public string BroadcasterName { get; set; }
        public CubeDraftStatus DraftStatus { get; set; }
        public bool IsBroadcaster { get; set; }
        public bool IsDrafting { get; set; }

        public IEnumerable<CardViewModel> Cards { get; set; }
        public IEnumerable<PlayerUpdatedModel> CubeDraftPlayers { get; set; } 
    }
}