using System;

namespace MTGO.Web.TournamentLibrary
{
    public class Pack
    {
        public Card[] Cards { get; internal set; }
        public string SetName { get; internal set; }

        public Pack(Card[] cards, string setName = null)
        {
            if ( cards.Length != 15 )
                throw new ArgumentOutOfRangeException("cards", "Invalid number of cards for pack");

            Cards = cards;
            SetName = setName;
        }
    }
}