using System;
using System.Data;
using System.Runtime.Serialization;
using MTGO.Common.Models;

namespace MTGO.Common.TournamentLibrary
{
    [Serializable]
    [KnownType(typeof(TournPlayer))]
    public class TournMatch : ITournMatch
    {
        public bool Equals(TournMatch other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Player1, other.Player1) && Equals(Player2, other.Player2) && Round == other.Round;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TournMatch)obj);
        }

        public override int GetHashCode()
        {
            return Player1.PlayerId ^ (Player2 != null ? Player2.PlayerId : 0) ^ Round;
        }

        public ITournPlayer Player1 { get; set; }
        public ITournPlayer Player2 { get; set; }

        public int Player1Wins = 0;
        public int Player2Wins = 0;
        public int Ties = 0;
        public int CurrentGame = 0;
        public int Round = 0;
        public int DBId;

        public bool ReportedResult
        {
            get { return CurrentGame == -1; }
        }

        public TournMatch()
        {
            Player1 = null;
            Player2 = null;
        }

        public TournMatch(ITournPlayer player1, ITournPlayer player2, int round)
        {
            if (player1 == null)
                throw new ArgumentException("Player1 cannot be null");

            if (player1 == player2)
                throw new ArgumentException("Player1 cannot equal player2");

            Player1 = player1;
            Player2 = player2;
            Round = round;

            if (player2 != null) return;

            Player1.Wins = 2;
            CurrentGame = -1;
        }

        public TournMatch(ITournPlayer player1, ITournPlayer player2, int round, int player1Wins, int player2Wins, int ties) : this(player1, player2, round)
        {
            Player1Wins = player1Wins;
            Player2Wins = player2Wins;
            Ties = ties;
        }

        public int CompareTo(object obj)
        {
            TournMatch match = (TournMatch)obj;

            return DBId.CompareTo(match.DBId);
        }

        public void FromDB(CubeDraftResult cubeDraftMatch)
        {
            var player1 = new Player();
            player1.FromDB(cubeDraftMatch.CubeDraftPlayer1);
            Player1 = new TournPlayer(player1);

            if (cubeDraftMatch.Player2Id == null)
            {
                Player2 = null;
            }
            else
            {
                var player2 = new Player();
                player2.FromDB(cubeDraftMatch.CubeDraftPlayer2);
                Player2 = new TournPlayer(player2);
            }
            CurrentGame = cubeDraftMatch.CurrentGame;
            Player1Wins = cubeDraftMatch.Player1Wins;
            Player2Wins = cubeDraftMatch.Player2Wins;
            Round = cubeDraftMatch.Round;
            DBId = cubeDraftMatch.Id;
        }

        public void ToDB(MainDbContext db, int draftId)
        {
            if (DBId > 0)
            {
                var match = db.CubeDraftResults.Find(DBId);

                match.Player1Id = Player1.PlayerId;
                match.Player2Id = Player2 == null ? (int?) null : Player2.PlayerId;
                match.CurrentGame = CurrentGame;
                match.Player1Wins = Player1Wins;
                match.Player2Wins = Player2Wins;
                match.Round = Round;
                match.DraftId = draftId;
                db.Entry(match).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                var newResult = new CubeDraftResult
                    {
                        Player1Id = Player1.PlayerId,
                        Player2Id = Player2 == null ? (int?) null : Player2.PlayerId,
                        CurrentGame = CurrentGame,
                        Player1Wins = Player1Wins,
                        Player2Wins = Player2Wins,
                        Round = Round,
                        DraftId = draftId
                    };
                db.CubeDraftResults.Add(newResult);
                db.SaveChanges();
                DBId = newResult.Id;
            }
        }

        public void ReportResult(IPlayer player, int wins, int losses, int ties)
        {
            if (Equals(Player1, player))
            {
                Player1Wins = wins;
                Player2Wins = losses;
            }
            else if (Equals(Player2, player))
            {
                Player2Wins = wins;
                Player1Wins = losses;
            }
            else
            {
                throw new ArgumentException("Player is not in this pairing");
            }

            Ties = ties;

            if (Player1Wins > Player2Wins)
            {
                Player1.Wins++;
                //Console.WriteLine("{0} Wins {1}", Player1.PlayerName, Player1.Wins);
            }
            else if (Player1Wins < Player2Wins)
            {
                Player2.Wins++;
                //Console.WriteLine("{0} Wins {1}", Player1.PlayerName, Player1.Wins);
            }
            CurrentGame = -1;
        }

        public override string ToString()
        {
            return Player2 == null ? String.Concat(Player1, " *** BYE ***") : String.Concat(Player1, " vs. ", Player2);
        }
    }
}