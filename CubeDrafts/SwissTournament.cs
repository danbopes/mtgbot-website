using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MTGOLibrary;

namespace MTGBotWebsite.CubeDrafts
{
    public class Player
    {
        //PlayerId in the Database
        public int PlayerId { get; internal set; }

        //PlayerName (Twitch username)
        public string PlayerName { get; internal set; }

        //Points
        public int Wins = 0;
        public int Losses = 0;
        public int Ties = 0;
        public int Points { get { return Wins*3 + Ties*1; } }

        public Player(int playerId, string playerName)
        {
            PlayerId = playerId;
            PlayerName = playerName;
        }

        public override string ToString()
        {
            return PlayerName;
        }
    }

    public class Pairing
    {
        public Player Player1 { get; internal set; }
        public Player Player2 { get; internal set; }

        public int Player1Wins = 0;
        public int Player2Wins = 0;
        public int Ties = 0;

        public Pairing(Player player1, Player player2)
        {
            if ( player1 == null )
                throw new ArgumentException("Player1 cannot be null");

            if ( player1 == player2 )
                throw new ArgumentException("Player1 cannot equal player2");

            Player1 = player1;
            Player2 = player2;

            if (player2 == null)
                Player1.Wins++;
        }

        public void ReportResult(Player player, int wins, int losses, int ties)
        {
            if (Player1 == player)
            {
                Player1Wins = wins;
                Player2Wins = losses;
            }
            else if (Player2 == player)
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
                Player1.Wins++;
            else if (Player1Wins < Player2Wins)
                Player2.Wins++;
        }

        public override string ToString()
        {
            return String.Concat(Player1, " vs. ", Player2);
        }
    }

    public class SwissTournament
    {
        public Player[] Standings
        {
            get { return Players.OrderByDescending(p => p.Points).ToArray(); }
        }

        public Player[] Players { get; internal set; }
        public int CurrentRound { get; internal set; }
        public Dictionary<int, Pairing[]> Rounds = new Dictionary<int, Pairing[]>(); 

        public SwissTournament(Player[] players)
        {
            Players = players;
            CurrentRound = 0;
        }

        public Pairing[] PairNextRound()
        {
            if ( Players.Length < 2 )
                throw new InvalidOperationException("Need more then 2 players to pair");

            var pairings = new List<Pairing>();

            var tempPlayers = ((Player[])Players.Clone()).Shuffle().OrderByDescending(p => p.Points).ToList();
            while ( tempPlayers.Count > 1 )
            {
                var player = tempPlayers.First();
                var previousOpponents = Rounds.SelectMany(i => i.Value)
                                              .Where(p => p.Player1 == player || p.Player2 == player)
                                              .Select(p => p.Player1 == player ? p.Player2 : p.Player1);

                //Find the best player
                var match = tempPlayers.FirstOrDefault(p => !previousOpponents.Contains(player) && p != player);

                if ( match == null )
                    throw new InvalidOperationException("Unable to find match for player.");

                pairings.Add(new Pairing(player, match));
                tempPlayers.Remove(player);
                tempPlayers.Remove(match);
            }

            if ( tempPlayers.Count > 0 )
                pairings.Add(new Pairing(tempPlayers.First(), null));

            CurrentRound++;

            Rounds.Add(CurrentRound, pairings.ToArray());
            return pairings.ToArray();
        }

        private float MatchWinPercentage(Player player)
        {
            return 0f;
        }

        private float GameWinPercentage(Player player)
        {
            return 0f;
        }

        private float OponentGameWinPercentage(Player player)
        {
            return 0f;
        }
    }
}
