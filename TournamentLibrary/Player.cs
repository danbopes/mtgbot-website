using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MTGBotWebsite.TournamentLibrary
{
    public class Player : IPlayer
    {
        //PlayerId in the Database
        public int PlayerId { get; internal set; }

        //PlayerName (Twitch username)
        public string PlayerName { get; internal set; }

        //MtgoId
        public int MtgoId { get; internal set; }

        //MtgoUsername
        public string MtgoUsername { get; internal set; }

        public Player()
        {
        }

        public Player(int playerId, string playerName, int mtgoId, string mtgoUsername)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            MtgoId = mtgoId;
            MtgoUsername = mtgoUsername;
        }

        public Player(IPlayer player) : this(player.PlayerId, player.PlayerName, player.MtgoId, player.MtgoUsername)
        {
        }

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return PlayerName;
        }
    }
}
