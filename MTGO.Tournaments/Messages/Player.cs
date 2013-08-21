using System;
using MTGO.Common.TournamentLibrary;
using MTGO.Database.Models.CubeDrafting;

namespace MTGO.Tournaments.Messages
{
    [Serializable]
    public class Player : IPlayer
    {
        public Player(int playerId, string playerName, int mtgoId, string mtgoUsername)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            MtgoId = mtgoId;
            MtgoUsername = mtgoUsername;
        }

        public Player(IPlayer player)
            : this(player.PlayerId, player.PlayerName, player.MtgoId, player.MtgoUsername)
        {
        }

        public Player(CubeDraftPlayer player)
        {
            PlayerId = player.Id;
            PlayerName = player.MtgoLink.User.Username;
            MtgoId = player.MtgoLink.Player.Id;
            MtgoUsername = player.MtgoLink.Player.Username;
        }
        
        public int PlayerId { get; internal set; }
        public string PlayerName { get; internal set; }
        public int MtgoId { get; internal set; }
        public string MtgoUsername { get; internal set; }

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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            var other = obj as Player;
            return other != null && Equals(other);
        }

        public override string ToString()
        {
            return PlayerName;
        }
    }
}
