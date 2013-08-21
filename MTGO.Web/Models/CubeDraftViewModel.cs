using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MTGO.Common.Models;

namespace MTGO.Web.Models
{
    public class CubeDraftViewModel
    {
        public string DraftName { get; set; }
        public string BroadcasterName { get; set; }
        public CubeDraftStatus DraftStatus { get; set; }
        public bool IsBroadcaster { get; set; }
        public bool IsDrafting { get; set; }

        public IEnumerable<CardViewModel> Cards { get; set; }
    }
}