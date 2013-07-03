using System;
using System.Collections.Generic;
using MTGOLibrary.Models;

namespace MTGBotWebsite.CubeDrafts
{
    public class Pack
    {
        public Card[] Cards { get; internal set; }
        public string SetName { get; internal set; }
        public int Number { get; internal set; }

        public Pack(Card[] cards, int packNumber, string setName = null)
        {
            if ( cards.Length != 15 )
                throw new ArgumentOutOfRangeException("cards", "Invalid number of cards for pack");

            if ( packNumber < 1 || packNumber > 3 )
                throw new ArgumentOutOfRangeException("packNumber", "Pack Number must be between 1 and 3");

            Cards = cards;
            Number = packNumber;
            SetName = setName;
        }
    }
}