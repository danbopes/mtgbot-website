using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MTGOLibrary.Models;

namespace MTGBotWebsite.CubeDrafts
{
    public interface ICubeDraftManager
    {
        List<Drafter> Players { get; set; }
        void Pick(string drafterName, int pickNumber, int pickId);
    }
}