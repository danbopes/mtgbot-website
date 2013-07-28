using System;
using System.Collections.Generic;
using System.Linq;
using MTGOLibrary;
using MTGOLibrary.TournamentLibrary;

namespace MTGBotWebsite.TournamentLibrary
{
    public class SwissTournament
    {
        public ITournPlayer[] Standings
        {
            get { return Players.OrderByDescending(p => p.Points).ToArray(); }
        }

        public TournPlayerArray Players { get; internal set; }
        public int CurrentRound { get; internal set; }
        public TournMatchArray Matches = new TournMatchArray();

        public bool OutstandingMatches { get { return Matches.GetByRound(CurrentRound).Any(m => !m.ReportedResult); } }

        public SwissTournament()
        {
            Players = new TournPlayerArray();

            CurrentRound = 0;
        }

        public void AddPlayer(IPlayer player)
        {
            Players.AddPlayer(new TournPlayer(player));
        }

        public void AddMatch(TournMatch match)
        {
            Matches.AddMatch(match);
            CurrentRound = Matches.Count == 0 ? 0 : Matches.Max(m => m.Round);
        }

        public void UpdateMatch(TournMatch matchToUpdate)
        {
            var match = Matches.FindIndex(matchToUpdate.Equals);

            if (match > -1)
                Matches[match] = matchToUpdate;
        }

        public object GetStandings()
        {
            var roundToGrab = CurrentRound;

            if (roundToGrab <= 0)
                return null;

            if (Matches.GetByRound(CurrentRound).Any(c => !c.ReportedResult))
            {
                roundToGrab = CurrentRound - 1;

                if (roundToGrab <= 0)
                    return null;
            }

            var matches = Matches.GetByRound(1, roundToGrab);

            foreach (var player in Players)
            {
                player.Losses = 0;
                player.GameLosses = 0;
                player.Wins = 0;
                player.GameWins = 0;
                player.Ties = 0;
            }

            foreach (var match in matches)
            {
                var player1 = Players.FindById(match.Player1.PlayerId);
                ITournPlayer player2 = null;
                player1.GameWins += match.Player1Wins;

                if (match.Player2 != null)
                {
                    player2 = Players.FindById(match.Player2.PlayerId);
                    player2.GameWins += match.Player2Wins;
                    player2.GameLosses += match.Player1Wins;
                    player1.GameLosses += match.Player2Wins;
                }

                if (match.Player2 == null || match.Player1Wins > match.Player2Wins)
                {
                    player1.Wins++;
                }
                else if (player2 != null && match.Player2Wins > match.Player1Wins)
                {
                    player2.Wins++;
                }
            }

            foreach (var player in Players)
            {
                var opponents =
                    matches.Where(p => p.Player2 != null && (p.Player1.PlayerId == player.PlayerId || p.Player2.PlayerId == player.PlayerId))
                           .Select(p => Players.FindById(new[] {p.Player1, p.Player2}.Single(sp => sp.PlayerId != player.PlayerId).PlayerId))
                           .Distinct()
                           .ToArray();


                player.TieBreaker1 = (float)Math.Round((opponents.Select(o => Math.Max(0.33f, (float) o.Points/(3*CurrentRound))).Sum()/
                                     opponents.Count())*100, 2);

                player.TieBreaker2 = (float)Math.Round(((float)player.GameWins/(player.GameWins + player.GameLosses))*100, 2);

                player.TieBreaker3 = (float)Math.Round((opponents.Select(o => Math.Max(0.33f, (float)o.GameWins / (o.GameWins+o.GameLosses))).Sum() /
                                     opponents.Count())*100, 2);

                if (float.IsNaN(player.TieBreaker1))
                    player.TieBreaker1 = 33.33f;

                if (float.IsNaN(player.TieBreaker2))
                    player.TieBreaker2 = 33.33f;

                if (float.IsNaN(player.TieBreaker3))
                    player.TieBreaker3 = 33.33f;
            }

            var i = 0;

            return Players
                .OrderByDescending(p => p.Points)
                .ThenByDescending(p => p.TieBreaker1)
                .ThenByDescending(p => p.TieBreaker2)
                .ThenByDescending(p => p.TieBreaker3)
                .Select(p => new
                    {
                        Place = ++i,
                        Player = p
                    }).ToArray();
        }

        public TournMatchArray PairNextRound()
        {
            if (Players.Count < 2)
                throw new InvalidOperationException("Need more then 2 players to pair.");

            if (CurrentRound > 0 && Matches.GetByRound(CurrentRound).Any(c => !c.ReportedResult))
                throw new InvalidOperationException("There are still outstanding results.");

            var pairings = new List<TournMatch>();
            var nextRound = CurrentRound + 1;

            var tempPlayers = Players.Shuffle().OrderByDescending(p => p.Points).ToList();

            //var tempPlayers = ((TournPlayer[])Players.Clone).Shuffle().OrderByDescending(p => p.Points).ToList();
            while (tempPlayers.Count > 1)
            {
                var player = tempPlayers.First();
                var previousOpponents = Matches.Where(p => p.Player1 == player || p.Player2 == player)
                                              .Select(p => p.Player1 == player ? p.Player2 : p.Player1);

                //Find the best player
                var match = tempPlayers.FirstOrDefault(p => !previousOpponents.Contains(player) && p != player);



                if (match == null)
                    throw new InvalidOperationException("Unable to find match for player.");

                TournMatch thispair = new TournMatch(player, match, nextRound);
                if (!previousOpponents.Contains(player))
                {
                    pairings.Add(thispair);
                    tempPlayers.Remove(player);
                    tempPlayers.Remove(match);
                }
                else
                {
                    //Console.WriteLine("Played Previously: {0}", thispair.ToString());
                    int samepoints = player.Wins;
                    var playertier =
                        pairings.Where(
                            tierpair => (tierpair.Player1.Wins == samepoints || tierpair.Player2.Wins == samepoints));

                    foreach (TournMatch thistierpair in playertier)
                    {
                        pairings.Remove(thistierpair);
                        tempPlayers.Add(thistierpair.Player1);
                        tempPlayers.Add(thistierpair.Player2);
                        tempPlayers.Shuffle();
                    }
                }
            }

            if (tempPlayers.Count > 0)
                pairings.Add(new TournMatch(tempPlayers.First(), null, nextRound));
            
            foreach (var pairing in pairings)
            {
                AddMatch(pairing);
            }

            return new TournMatchArray(pairings);
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