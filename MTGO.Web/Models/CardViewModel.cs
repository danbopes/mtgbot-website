using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MTGO.Web.Models
{
    public class CardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SetName { get; set; }
        public string MagicCardsInfoId { get; set; }
        public bool Foil { get; set; }
        public int CardTextureNumber { get; set; }
        public int? CardSetId { get; set; }
        public string Rarity { get; set; }
        public string ManaCost { get; set; }
        public int CMC { get; set; }
        public string Color { get; set; }
    }
}