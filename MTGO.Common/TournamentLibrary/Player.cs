using System;
using System.Runtime.Serialization;
using MTGO.Common.Entities.CubeDrafting;

namespace MTGO.Common.TournamentLibrary
{
    [Serializable]
    [KnownType(typeof(TournPlayer))]
    public class Player : IPlayer
    {
        protected bool Equals(Player other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(PlayerId, other.PlayerId);
        }

        public override int GetHashCode()
        {
            return PlayerId;
        }

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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as Player;
            return other != null && Equals(other);
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

        public void FromDB(CubeDraftPlayer player)
        {
            PlayerId = player.Id;
            PlayerName = player.MtgoLink.User.TwitchUsername;
            MtgoId = player.MtgoLink.MtgoId;
            MtgoUsername = player.MtgoLink.MtgoUsername;
        }

        public override string ToString()
        {
            return PlayerName;
        }
    }
}
